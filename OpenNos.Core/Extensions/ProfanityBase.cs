using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core.Extensions
{
    public partial class ProfanityBase
    {
        protected List<string> _profanities;

        public ProfanityBase()
        {
            _profanities = new List<string>(_wordList);
        }

        protected ProfanityBase(string[] profanityList)
        {
            if (profanityList == null) return;

            _profanities = new List<string>(profanityList);
        }

        protected ProfanityBase(List<string> profanityList)
        {
            if (profanityList == null) return;

            _profanities = profanityList;
        }

        public void AddProfanity(string profanity)
        {
            if (string.IsNullOrEmpty(profanity)) return;

            _profanities.Add(profanity);
        }

        public void AddProfanity(string[] profanityList)
        {
            if (profanityList == null) return;

            _profanities.AddRange(profanityList);
        }

        public void AddProfanity(List<string> profanityList)
        {
            if (profanityList == null) return;

            _profanities.AddRange(profanityList);
        }

        public bool RemoveProfanity(string profanity)
        {
            if (string.IsNullOrEmpty(profanity)) return false;

            return _profanities.Remove(profanity.ToLower(CultureInfo.InvariantCulture));
        }

        public bool RemoveProfanity(List<string> profanities)
        {
            if (profanities == null) return false;

            foreach (string naughtyWord in profanities)
            {
                if (!RemoveProfanity(naughtyWord))
                {
                    return false;
                }
            }

            return true;
        }

        public bool RemoveProfanity(string[] profanities)
        {
            if (profanities == null) return false;

            foreach (string naughtyWord in profanities)
            {
                if (!RemoveProfanity(naughtyWord))
                {
                    return false;
                }
            }

            return true;
        }

        public void Clear()
        {
            _profanities.Clear();
        }

        public int Count
        {
            get
            {
                return _profanities.Count;
            }
        }
    }
}
