using NosByte.Packets.ClientPackets;
using OpenNos.Core;
using OpenNos.Core.Logger;
using OpenNos.DAL;
using OpenNos.Data;
using OpenNos.Domain;
using OpenNos.GameObject;
using OpenNos.Master.Library.Client;
using System;
using System.Configuration;
using System.Linq;

namespace OpenNos.Handler.Login
{
    public class LoginPacketHandler : IPacketHandler
    {
        private ClientSession Session { get; set; }

        public LoginPacketHandler(ClientSession session) => Session = session;

        /// <summary>
        /// login packet
        /// </summary>
        /// <param name="loginPacket"></param>
        public void VerifyLogin(LoginPacket loginPacket)
        {
            if (loginPacket == null || loginPacket.Name == null || loginPacket.Password == null)
            {
                Session.PacketHandlerInterval?.Dispose();
                return;
            }

            UserDTO user = new UserDTO
            {
                Name = loginPacket.Name,
                Password = ConfigurationManager.AppSettings["UseOldCrypto"] == "true"
                    ? CryptographyBase.Sha512(LoginCryptography.GetPassword(loginPacket.Password)).ToUpper()
                    : loginPacket.Password
            };
            if (user.Name == null || user.Password == null)
            {
                Session.PacketHandlerInterval?.Dispose();
                return;
            }

            var version = ConfigurationManager.AppSettings["ClientVersion"];

            if (version != loginPacket.ClientData)
            {
                Logger.Log.Warn($"Client version: {loginPacket.ClientData}");
                Logger.Log.Warn($"Required version: {version}");
                Session.SendPacket($"failc {(byte)LoginFailType.OldClient}");
                Session.PacketHandlerInterval?.Dispose();
                return;
            }

            AccountDTO loadedAccount = DAOFactory.AccountDAO.LoadByName(user.Name);
            if (loadedAccount != null && loadedAccount.Name != user.Name)
            {
                Session.SendPacket($"failc {(byte)LoginFailType.WrongCaps}");
                Session.PacketHandlerInterval?.Dispose();
                return;
            }

            if (loadedAccount?.Password.ToUpper().Equals(user.Password) != true)
            {
                Session.SendPacket($"failc {(byte)LoginFailType.AccountOrPasswordWrong}");
                Session.PacketHandlerInterval?.Dispose();
                return;
            }

            string ipAddress = Session.CleanIpAddress; 

            DAOFactory.AccountDAO.WriteGeneralLog(loadedAccount.AccountId, ipAddress, null,
                               GeneralLogType.Connection, "LoginServer");

            if (DAOFactory.PenaltyLogDAO.LoadByIp(ipAddress).Any())
            {
                Session.SendPacket($"failc {(byte)LoginFailType.CantConnect}");
                Session.PacketHandlerInterval?.Dispose();
                return;
            }

            //check if the account is connected
            if (CommunicationServiceClient.Instance.IsAccountConnected(loadedAccount.AccountId))
            {
                Session.SendPacket($"failc {(byte)LoginFailType.AlreadyConnected}");
                Session.PacketHandlerInterval?.Dispose();
                return;
            }

            AuthorityType type = loadedAccount.Authority;
            PenaltyLogDTO penalty = DAOFactory.PenaltyLogDAO.LoadByAccount(loadedAccount.AccountId)
                                              .FirstOrDefault(s =>
                                                      s.DateEnd > DateTime.Now &&
                                                      s.Penalty == PenaltyType.Banned);
            if (penalty != null)
            {
                Session.SendPacket($"failc {(byte)LoginFailType.Banned}");
                Session.PacketHandlerInterval?.Dispose();
                return;
            }

            switch (type)
            {
                case AuthorityType.Unconfirmed:
                    {
                        Session.SendPacket($"failc {(byte)LoginFailType.CantConnect}");
                        Session.PacketHandlerInterval?.Dispose();
                    }
                    break;

                case AuthorityType.Banned:
                    {
                        Session.SendPacket($"failc {(byte)LoginFailType.Banned}");
                        Session.PacketHandlerInterval?.Dispose();
                    }
                    break;

                case AuthorityType.Closed:
                    {
                        Session.SendPacket($"failc {(byte)LoginFailType.CantConnect}");
                        Session.PacketHandlerInterval?.Dispose();
                    }
                    break;

                default:
                    {
                        if (loadedAccount.Authority < AuthorityType.TGS)
                        {
                            MaintenanceLogDTO maintenanceLog = DAOFactory.MaintenanceLogDAO.LoadFirst();
                            if (maintenanceLog != null && maintenanceLog.DateStart < DateTime.Now)
                            {
                                Session.SendPacket($"failc {(byte)LoginFailType.Maintenance}");
                                Session.PacketHandlerInterval?.Dispose();
                                return;
                            }
                        }

                        int newSessionId = SessionFactory.Instance.GenerateSessionId();
                        Logger.Log.Debug(string.Format(Language.Instance.GetMessageFromKey("CONNECTION"), user.Name,
                                newSessionId));
                        try
                        {
                            CommunicationServiceClient.Instance.RegisterAccountLogin(loadedAccount.AccountId,
                                    newSessionId, ipAddress);
                        }
                        catch (Exception ex)
                        {
                            Logger.Log.Error("General Error SessionId: " + newSessionId, ex);
                        }

                        string[] clientData = loginPacket.ClientData.Split('.');

                        if (clientData.Length < 2)
                        {
                            clientData = loginPacket.ClientDataOld.Split('.');
                        }

                        bool ignoreUserName = short.TryParse(clientData[3], out short clientVersion)
                                           && (clientVersion < 3075
                                            || ConfigurationManager.AppSettings["UseOldCrypto"] == "true");

                        //check if the account is connected
                        if (CommunicationServiceClient.Instance.IsAccountConnected(loadedAccount.AccountId))
                        {
                            Session.SendPacket($"failc {(byte)LoginFailType.AlreadyConnected}");
                            Session.PacketHandlerInterval?.Dispose();
                            return;
                        }
                        Session.SendPacket(BuildServersPacket(user.Name, newSessionId, ignoreUserName));
                        Session.PacketHandlerInterval?.Dispose();
                    }
                    break;
            }
        }

        private string BuildServersPacket(string username, int sessionId, bool ignoreUserName)
        {
            string channelpacket =
                CommunicationServiceClient.Instance.RetrieveRegisteredWorldServers(username, sessionId, ignoreUserName);

            if (channelpacket == null || !channelpacket.Contains(':'))
            {
                Logger.Log.Debug(
                    "Could not retrieve Worldserver groups. Please make sure they've already been registered.");
                Session.SendPacket($"failc {(byte)LoginFailType.Maintenance}");
            }

            return channelpacket;
        }
    }
}
