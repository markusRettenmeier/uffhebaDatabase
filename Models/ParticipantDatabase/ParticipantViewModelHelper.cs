namespace Sammlerplattform.Models.ParticipantDatabase
{
    public class ParticipantViewModelHelper
    {
        public static ParticipantViewModel FromDomainModel(Participant participant)
        {
            string places = participant.ParticipantNPlaceList != null ?
                        string.Join(", ", participant.ParticipantNPlaceList
                        .Select(x => x.Place.PlaceNToponymyList.Where(t => t.IsCurrentName)
                        .Select(t => t.Toponymy.ToponymyName).FirstOrDefault())) : "Keine Orte zugewiesen";
            string industry = participant.Organization?.Industry != null ?
                        participant.Organization.Industry.IndustryName : string.Empty;

            ParticipantViewModel particpantViewModel = new()
            {
                Name = participant.ParticipantName,
                Pseudonym = participant.Individual?.Pseudonym ?? string.Empty,
                Signature = participant.Individual?.Signature ?? string.Empty,
                Places = string.Join(", ", places),
                Industry = industry,
                IsDeletable = participant.CollectionItemNParticipantList.Count == 0
            };

            if (!string.IsNullOrEmpty(particpantViewModel.Pseudonym))
            {
                particpantViewModel.FurtherSpecs = particpantViewModel.Pseudonym + ", ";
            }
            if (!string.IsNullOrEmpty(particpantViewModel.Signature))
            {
                particpantViewModel.FurtherSpecs = particpantViewModel.Signature + ", ";
            }
            if (!string.IsNullOrEmpty(particpantViewModel.Places))
            {
                particpantViewModel.FurtherSpecs = particpantViewModel.Places + ", ";
            }
            if (!string.IsNullOrEmpty(particpantViewModel.Industry))
            {
                particpantViewModel.FurtherSpecs = particpantViewModel.Industry + ", ";
            }

            return particpantViewModel;
        }
    }
    public class ParticipantViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Pseudonym { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string Places { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string FurtherSpecs { get; set; } = string.Empty;
        public bool IsDeletable { get; set; } = false;
    }
}
