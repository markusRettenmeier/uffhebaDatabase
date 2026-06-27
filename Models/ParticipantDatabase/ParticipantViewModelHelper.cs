namespace Sammlerplattform.Models.ParticipantDatabase
{
    public class ParticipantViewModelHelper
    {
        public static ParticipantViewModel FromDomainModel(ParticipantDisplayDTO participant)
        {
            string places = participant.ConnectedPlaceList != null ?
                        string.Join(", ", participant.ConnectedPlaceList
                        .Select(x => x.ToponymyList.Where(t => t.IsCurrentName)
                        .Select(t => t.Name).FirstOrDefault())) : "Keine Orte zugewiesen";
            string industry = participant.IndustryName ?? string.Empty;

            ParticipantViewModel particpantViewModel = new()
            {
                Name = participant.Name,
                Pseudonym = participant.Pseudonym ?? string.Empty,
                Signature = participant.Signature ?? string.Empty,
                Places = string.Join(", ", places),
                Industry = industry,
                IsDeletable = participant.CollectionItemNParticipantList.ToList().Count == 0
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
