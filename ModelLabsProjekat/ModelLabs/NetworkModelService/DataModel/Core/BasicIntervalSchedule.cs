using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
    public class BasicIntervalSchedule : IdentifiedObject
    {
        private DateTime startTime ;
        private UnitMultiplier value1Multiplier;
        private UnitSymbol value1Unit;
        private UnitMultiplier value2Multiplier;
        private UnitSymbol value2Unit;

        public BasicIntervalSchedule(long globalId)
            : base(globalId)
        {
        }

        public DateTime StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        public UnitMultiplier Value1Multiplier
        {
            get { return value1Multiplier; }
            set { value1Multiplier = value; }
        }

        public UnitSymbol Value1Unit
        {
            get { return value1Unit; }
            set { value1Unit = value; }
        }

        public UnitMultiplier Value2Multiplier
        {
            get { return value2Multiplier; }
            set { value2Multiplier = value; }
        }

        public UnitSymbol Value2Unit
        {
            get { return value2Unit; }
            set { value2Unit = value; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                BasicIntervalSchedule x = (BasicIntervalSchedule)obj;
                return (x.startTime == this.startTime &&
                        x.value1Multiplier == this.value1Multiplier &&
                        x.value1Unit == this.value1Unit &&
                        x.value2Multiplier == this.value2Multiplier &&
                        x.value2Unit == this.value2Unit);
            }
            else
            {
                return false;
            }
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
                case ModelCode.BIS_STARTTIME:
                case ModelCode.BIS_VAL1MUL:
                case ModelCode.BIS_VAL1UNIT:
                case ModelCode.BIS_VAL2MUL:
                case ModelCode.BIS_VAL2UNIT:
                    return true;

                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.BIS_STARTTIME:
                    property.SetValue(startTime);
                    break;
                case ModelCode.BIS_VAL1MUL:
                    property.SetValue((short)value1Multiplier);
                    break;
                case ModelCode.BIS_VAL1UNIT:
                    property.SetValue((short)value1Unit);
                    break;
                case ModelCode.BIS_VAL2MUL:
                    property.SetValue((short)value2Multiplier);
                    break;
                case ModelCode.BIS_VAL2UNIT:
                    property.SetValue((short)value2Unit);
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
                case ModelCode.BIS_STARTTIME:
                    startTime = property.AsDateTime();
                    break;
                case ModelCode.BIS_VAL1MUL:
                    value1Multiplier = (UnitMultiplier)property.AsEnum();
                    break;
                case ModelCode.BIS_VAL1UNIT:
                    value1Unit = (UnitSymbol)property.AsEnum();
                    break;
                case ModelCode.BIS_VAL2MUL:
                    value2Multiplier = (UnitMultiplier)property.AsEnum();
                    break;
                case ModelCode.BIS_VAL2UNIT:
                    value2Unit = (UnitSymbol)property.AsEnum();
                    break;
                default:
                    base.SetProperty(property);
                    break;
            }
        }

        #endregion IAccess implementation
    }
}
