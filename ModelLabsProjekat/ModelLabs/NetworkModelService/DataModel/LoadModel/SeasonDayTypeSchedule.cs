using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.DataModel.LoadModel
{
    public class SeasonDayTypeSchedule : Core.RegularIntervalSchedule
    {
        // Property koji čuva referencu na DayType
        private long dayType = 0;

        public SeasonDayTypeSchedule(long globalId)
            : base(globalId)
        {
        }

        public long DayType
        {
            get { return dayType; }
            set { dayType = value; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                SeasonDayTypeSchedule x = (SeasonDayTypeSchedule)obj;
                return this.dayType == x.dayType;
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
                // KAŽEMO DA OVA KLASA IMA OVAJ PROPERTY
                case ModelCode.SEASON_DAYTYPE_DAYTYPE:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.SEASON_DAYTYPE_DAYTYPE:
                    property.SetValue(dayType);
                    break;
                default:
                    base.GetProperty(property);
                    break;
            }
        }

        public override void SetProperty(Property property)
        {
            switch (property.Id)
            {
                // KADA SE POSTAVLJA PROPERTY, SAČUVAMO VREDNOST
                case ModelCode.SEASON_DAYTYPE_DAYTYPE:
                    dayType = property.AsReference();
                    break;
                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation

        #region IReference implementation

        public override void GetReferences(Dictionary<ModelCode, List<long>> references, TypeOfReference refType)
        {
            // VRAĆAMO REFERENCU KAO 'REFERENCE' (jer ovaj objekat pokazuje na drugi)
            if (dayType != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.SEASON_DAYTYPE_DAYTYPE] = new List<long> { dayType };
            }

            base.GetReferences(references, refType);
        }

        #endregion IReference implementation
    }
}