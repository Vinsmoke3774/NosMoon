using OpenNos.Data;
using OpenNos.GameObject.Networking;
using System.Linq;
using OpenNos.GameObject.Extension.CharacterExtensions;

namespace OpenNos.GameObject.Helpers
{
    public static class TitleHelper
    {
        #region Methods

        public static void GetEffectFromTitle(this Character e)
        {
            e.EffectFromTitle.Clear();

            long tit = 0;
            if (e.Title.Find(s => s.Stat.Equals(5)) != null)
            {
                tit = e.Title.Find(s => s.Stat.Equals(5)).TitleVnum;
            }
            if (e.Title.Find(s => s.Stat.Equals(7)) != null)
            {
                tit = e.Title.Find(s => s.Stat.Equals(7)).TitleVnum;
            }

            Item item = ServerManager.GetItem((short)tit);

            if (item == null) return;

            foreach (var bcard in item.BCards)
            {
                e.EffectFromTitle.Add(bcard);
            }
        }

        public static void GetVisualFromTitle(this Character e)
        {
            e.VisualFromTitle.Clear();

            long tit = 0;
            if (e.Title.Find(s => s.Stat.Equals(5)) != null)
            {
                tit = e.Title.Find(s => s.Stat.Equals(5)).TitleVnum;
            }
            if (e.Title.Find(s => s.Stat.Equals(7)) != null)
            {
                tit = e.Title.Find(s => s.Stat.Equals(7)).TitleVnum;
            }

            Item item = ServerManager.GetItem((short)tit);

            if (item == null) return;

            foreach (var bcard in item.BCards)
            {
                e.VisualFromTitle.Add(bcard);
            }
        }

        //public static void GetTitleFromLevel(this Character e)
        //{
        //    e.GetVnumAndLevel(9300, 1);
        //    e.GetVnumAndLevel(9301, 10);
        //    e.GetVnumAndLevel(9302, 20);
        //    e.GetVnumAndLevel(9303, 30);
        //    e.GetVnumAndLevel(9304, 40);
        //    e.GetVnumAndLevel(9305, 50);
        //    e.GetVnumAndLevel(9306, 60);
        //    e.GetVnumAndLevel(9307, 70);
        //    e.GetVnumAndLevel(9308, 80);
        //    e.GetVnumAndLevel(9309, 90);
        //}

        public static void GetVnumAndLevel(this Character e, short vnum, int lvl)
        {
            if (e.Title.Any(s => s.TitleVnum == vnum))
            {
                return;
            }

            if (e.Level < lvl)
            {
                return;
            }

            e.Title.Add(new CharacterTitleDTO
            {
                CharacterId = e.CharacterId,
                Stat = 1,
                TitleVnum = vnum
            });

            e.Session.SendPacket(e.GenerateTitle());
        }

        #endregion
    }
}