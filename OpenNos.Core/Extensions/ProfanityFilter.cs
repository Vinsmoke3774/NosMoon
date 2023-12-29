using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenNos.Core.Extensions
{
    public class ProfanityFilter : ProfanityBase
    {
        public ProfanityFilter()
        {
            AllowList = new AllowList();
        }

        public ProfanityFilter(string[] profanityList) : base(profanityList)
        {
            AllowList = new AllowList();
        }

        public ProfanityFilter(List<string> profanityList) : base(profanityList)
        {
            AllowList = new AllowList();
        }

        public AllowList AllowList { get; set; }

        public bool IsProfanity(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;

            if (AllowList.Contains(word.ToLower(CultureInfo.InvariantCulture))) return false;

            return _profanities.Contains(word.ToLower(CultureInfo.InvariantCulture));
        }

        public ReadOnlyCollection<string> DetectAllProfanities(string sentence)
        {
            return DetectAllProfanities(sentence, false);
        }

        public ReadOnlyCollection<string> DetectAllProfanities(string sentence, bool removePartialMatches)
        {
            if (string.IsNullOrEmpty(sentence)) return new(new List<string>());

            sentence = sentence.ToLower();
            sentence = sentence.Replace(".", "");
            sentence = sentence.Replace(",", "");

            var words = sentence.Split(' ');
            var postAllowList = FilterWordListByAllowList(words);
            List<string> swearList = new();

            AddMultiWordProfanities(swearList, ConvertWordListToSentence(postAllowList));

            if (removePartialMatches)
            {
                swearList.RemoveAll(x => swearList.Any(y => x != y && y.Contains(x)));
            }

            return new ReadOnlyCollection<string>(FilterSwearListForCompleteWordsOnly(sentence, swearList).Distinct().ToList());
        }

        public string CensorString(string sentence)
        {
            return CensorString(sentence, '*');
        }

        public string CensorString(string sentence, char censorCharacter)
        {
            return CensorString(sentence, censorCharacter, false);
        }

        public string CensorString(string sentence, char censorCharacter, bool ignoreNumbers)
        {
            if (string.IsNullOrEmpty(sentence))
            {
                return string.Empty;
            }

            string noPunctuation = sentence.Trim();
            noPunctuation = noPunctuation.ToLower();

            noPunctuation = Regex.Replace(noPunctuation, @"[^\w\s]", "");

            var words = noPunctuation.Split(' ');

            var postAllowList = FilterWordListByAllowList(words);
            var swearList = new List<string>();

            AddMultiWordProfanities(swearList, ConvertWordListToSentence(postAllowList));


            StringBuilder censored = new StringBuilder(sentence);
            StringBuilder tracker = new StringBuilder(sentence);

            return CensorStringByProfanityList(censorCharacter, swearList, censored, tracker, ignoreNumbers).ToString();
        }

        public (int, int, string)? GetCompleteWord(string toCheck, string profanity)
        {
            if (string.IsNullOrEmpty(toCheck))
            {
                return null;
            }

            string profanityLowerCase = profanity.ToLower(CultureInfo.InvariantCulture);
            string toCheckLowerCase = toCheck.ToLower(CultureInfo.InvariantCulture);

            if (toCheckLowerCase.Contains(profanityLowerCase))
            {
                var startIndex = toCheckLowerCase.IndexOf(profanityLowerCase, StringComparison.Ordinal);
                var endIndex = startIndex;

                while (startIndex > 0)
                {
                    if (toCheck[startIndex - 1] == ' ' || char.IsPunctuation(toCheck[startIndex - 1]))
                    {
                        break;
                    }

                    startIndex -= 1;
                }

                while (endIndex < toCheck.Length)
                {
                    if (toCheck[endIndex] == ' ' || char.IsPunctuation(toCheck[endIndex]))
                    {
                        break;
                    }

                    endIndex += 1;
                }

                return (startIndex, endIndex, toCheckLowerCase.Substring(startIndex, endIndex - startIndex).ToLower(CultureInfo.InvariantCulture));
            }

            return null;
        }

        public bool ContainsProfanity(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return false;
            }

            List<string> potentialProfanities = _profanities.Where(word => word.Length <= term.Length).ToList();

            if (potentialProfanities.Count == 0)
            {
                return false;
            }

            string regexPattern = string.Format(@"(?:{0})", string.Join("|", potentialProfanities).Replace("$", "\\$"));

            foreach (Match profanity in Regex.Matches(term, regexPattern, RegexOptions.IgnoreCase))
            {
                if (!AllowList.Contains(profanity.Value.ToLower(CultureInfo.InvariantCulture)))
                {
                    return true;
                }
            }

            return false;
        }

        private StringBuilder CensorStringByProfanityList(char censorCharacter, List<string> swearList, StringBuilder censored, StringBuilder tracker, bool ignoreNumeric)
        {
            foreach (string word in swearList.OrderByDescending(x => x.Length))
            {
                (int, int, string)? result = (0, 0, "");
                var multiWord = word.Split(' ');

                if (multiWord.Length == 1)
                {
                    do
                    {
                        result = GetCompleteWord(tracker.ToString(), word);

                        if (result != null)
                        {
                            string filtered = result.Value.Item3;

                            if (ignoreNumeric)
                            {
                                filtered = Regex.Replace(result.Value.Item3, @"[\d-]", string.Empty);
                            }

                            if (filtered == word)
                            {
                                for (int i = result.Value.Item1; i < result.Value.Item2; i++)
                                {
                                    censored[i] = censorCharacter;
                                    tracker[i] = censorCharacter;
                                }
                            }
                            else
                            {
                                for (int i = result.Value.Item1; i < result.Value.Item2; i++)
                                {
                                    tracker[i] = censorCharacter;
                                }
                            }
                        }
                    }
                    while (result != null);
                }
                else
                {
                    censored = censored.Replace(word, CreateCensoredString(word, censorCharacter));
                }
            }

            return censored;
        }

        private List<string> FilterSwearListForCompleteWordsOnly(string sentence, List<string> swearList)
        {
            List<string> filteredSwearList = new List<string>();
            StringBuilder tracker = new(sentence);

            foreach (string word in swearList.OrderByDescending(x => x.Length))
            {
                (int, int, string)? result = (0, 0, "");
                var multiWord = word.Split(' ');

                if (multiWord.Length == 1)
                {
                    do
                    {
                        result = GetCompleteWord(tracker.ToString(), word);

                        if (result != null)
                        {
                            if (result.Value.Item3 == word)
                            {
                                filteredSwearList.Add(word);

                                for (int i = result.Value.Item1; i < result.Value.Item2; i++)
                                {
                                    tracker[i] = '*';
                                }
                                break;
                            }

                            for (int i = result.Value.Item1; i < result.Value.Item2; i++)
                            {
                                tracker[i] = '*';
                            }
                        }
                    }
                    while (result != null);
                }
                else
                {
                    filteredSwearList.Add(word);
                    tracker.Replace(word, " ");
                }
            }

            return filteredSwearList;
        }

        private List<string> FilterWordListByAllowList(string[] words)
        {
            List<string> postAllowList = new();
            foreach (string word in words)
            {
                if (!string.IsNullOrEmpty(word))
                {
                    if (!AllowList.Contains(word.ToLower(CultureInfo.InvariantCulture)))
                    {
                        postAllowList.Add(word);
                    }
                }
            }

            return postAllowList;
        }

        private static string ConvertWordListToSentence(List<string> postAllowList)
        {
            string postAllowListSentence = string.Empty;

            foreach (string w in postAllowList)
            {
                postAllowListSentence = postAllowListSentence + w + " ";
            }

            return postAllowListSentence;
        }

        private void AddMultiWordProfanities(List<string> swearList, string postAllowListSentence)
        {
            swearList.AddRange(
                from string profanity in _profanities
                where postAllowListSentence.ToLower(CultureInfo.InvariantCulture).Contains(profanity)
                select profanity);
        }

        private static string CreateCensoredString(string word, char censorCharacter)
        {
            string censoredWord = string.Empty;

            for (int i = 0; i < word.Length; i++)
            {
                if (word[i] != ' ')
                {
                    censoredWord += censorCharacter;
                }
                else
                {
                    censoredWord += ' ';
                }
            }

            return censoredWord;
        }

    }
}
