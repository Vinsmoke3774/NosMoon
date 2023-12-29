using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace OpenNos.DAL.DAO
{
    public class Participant
    {
        public static double DefaultMean { get; } = 25.0;

        public static double DefaultStandardDeviation { get; } = 25.0 / 3.0;

        public Participant()
        {
            Mean = DefaultMean;
            StandardDeviation = DefaultStandardDeviation;
            Rating = 0;
        }

        [Key]
        public int ParticipantId { get; set; }

        public string Name { get; set; }

        public double Mean { get; set; }

        public double StandardDeviation { get; set; }

        public double Rating { get; set; }

        public bool AddParcipant(string[] arguments)
        {
            if (arguments.Length < 1 || arguments.Length > 2)
            {
                return false;
            }

            int score = 0;
            if (arguments.Length == 2 && !int.TryParse(arguments[1], out score))
            {
                return false;
            }

            using EF.OpenNosContext context = new();
            if (context.Participants.Where((p) => p.Name == arguments[0]).Any())
            {
                return false;
            }

            //context.Participants.Add(new Participant
            //{
            //    Name = arguments[0],
            //    Mean = arguments.Length == 2
            //        ? score
            //        : Participant.DefaultMean,
            //    StandardDeviation = arguments.Length == 2
            //        ? score / 3.0
            //        : Participant.DefaultStandardDeviation,
            //    Rating = arguments.Length == 2
            //        ? new Moserware.Skills.Rating(
            //            score, score / 3.0).ConservativeRating
            //        : Participant.DefaultStandardDeviation
            //});

            context.SaveChanges();
            return true;
        }

        public Participant FindParticipantByString(string participantName)
        {
            using EF.OpenNosContext context = new();
            Participant participant = context.Participants
                .Where((p) => p.Name == participantName.ToLower())
                .FirstOrDefault();

            if (participant != null)
                return participant;

            if (int.TryParse(participantName, out int participantID))
            {
                return context.Participants.Find(participantID);
            }

            return null;
        }
    }
}
