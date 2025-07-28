using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.DataModel.LoadModel
{
    public class DayType : IdentifiedObject
    {
        // Lista koja čuva GID-ove SeasonDayTypeSchedule-ova koji referenciraju ovaj DayType
        private List<long> seasonDayTypeSchedules = new List<long>();

        public DayType(long globalId)
            : base(globalId)
        {
        }

        public List<long> SeasonDayTypeSchedules
        {
            get { return seasonDayTypeSchedules; }
            set { seasonDayTypeSchedules = value; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                DayType x = (DayType)obj;
                // Poredi i listu referenci
                return CompareHelper.CompareLists(this.seasonDayTypeSchedules, x.seasonDayTypeSchedules, true);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #region IAccess implementation

        public override bool HasProperty(ModelCode property)
        {
            switch (property)
            {
                // Property za listu referenci
                case ModelCode.DAYTYPE_SEASON_DAYTYPE:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.DAYTYPE_SEASON_DAYTYPE:
                    property.SetValue(seasonDayTypeSchedules);
                    break;
                default:
                    base.GetProperty(property);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            // SetProperty se obično ne koristi za liste referenci, one se pune preko Add/RemoveReference
            base.SetProperty(property);
        }

        #endregion IAccess implementation

        #region IReference implementation

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            // Vraća listu GID-ova kao TARGET reference (jer drugi objekti pokazuju na ovaj)
            if (seasonDayTypeSchedules != null && seasonDayTypeSchedules.Count > 0 &&
                (refType == TypeOfReference.Target || refType == TypeOfReference.Both))
            {
                references[ModelCode.DAYTYPE_SEASON_DAYTYPE] = seasonDayTypeSchedules.GetRange(0, seasonDayTypeSchedules.Count);
            }

            base.GetReferences(references, refType);
        }

        // *** OVA METODA JE NEDOSTAJALA ***
        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                // Kada SeasonDayTypeSchedule postavi referencu na ovaj DayType...
                case ModelCode.SEASON_DAYTYPE_DAYTYPE:
                    seasonDayTypeSchedules.Add(globalId);
                    break;

                default:
                    base.AddReference(referenceId, globalId);
                    break;
            }
        }

        // *** I OVA METODA JE NEDOSTAJALA ***
        public override void RemoveReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.SEASON_DAYTYPE_DAYTYPE:
                    if (seasonDayTypeSchedules.Contains(globalId))
                    {
                        seasonDayTypeSchedules.Remove(globalId);
                    }
                    else
                    {
                        CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Entity (GID = 0x{0:x16}) doesn't contain reference 0x{1:x16}.", this.GlobalId, globalId);
                    }
                    break;

                default:
                    base.RemoveReference(referenceId, globalId);
                    break;
            }
        }

        #endregion IReference implementation
    }
}

