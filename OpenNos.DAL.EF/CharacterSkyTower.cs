using System.ComponentModel.DataAnnotations;

namespace OpenNos.DAL.EF
{
    public class CharacterSkyTower
    {
        #region Properties

        [Key]

        public int Id { get; set; }

        public long CharacterId { get; set; }

        public byte Timer { get; set; }

        public byte Round { get; set; }

        #endregion
    }
}