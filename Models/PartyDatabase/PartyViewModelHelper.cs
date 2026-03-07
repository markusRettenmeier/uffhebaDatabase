namespace Sammlerplattform.Models.PartyDatabase
{
    public class PartyViewModelHelper
    {
        public static PartyViewModel FromDomainModel(Party party)
        {
            //string places = party.PlaceList != null ?
            //            string.Join(", ", party.PlaceList.Select(p => p.PlaceNToponymyList.Where(t => t.IsCurrentName).Select(t => t.Toponymy.ToponymyName).FirstOrDefault())) : "Keine Orte zugewiesen";
            string industry = party.Organization?.Industry != null ?
                        party.Organization.Industry.IndustryName : string.Empty;

            PartyViewModel partyViewModel = new()
            {
                Name = party.PartyName,
                Pseudonym = party.Individual?.Pseudonym ?? string.Empty,
                Signature = party.Individual?.Signature ?? string.Empty,
                //Places = string.Join(", ", places),
                Industry = industry
            };

            if (!string.IsNullOrEmpty(partyViewModel.Pseudonym))
            {
                partyViewModel.FurtherSpecs = partyViewModel.Pseudonym + ", ";
            }
            if (!string.IsNullOrEmpty(partyViewModel.Signature))
            {
                partyViewModel.FurtherSpecs = partyViewModel.Signature + ", ";
            }
            //if (!string.IsNullOrEmpty(partyViewModel.Places))
            //{
            //    partyViewModel.FurtherSpecs = partyViewModel.Places + ", ";
            //}
            if (!string.IsNullOrEmpty(partyViewModel.Industry))
            {
                partyViewModel.FurtherSpecs = partyViewModel.Industry + ", ";
            }

            return partyViewModel;
        }
    }
    public class PartyViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string Pseudonym { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string Places { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string FurtherSpecs { get; set; } = string.Empty;
    }
}
