using FTN.Common;
using FTN.Services.NetworkModelService.DataModel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.DataModel.Wires
{
    public class RegulatingControl : PowerSystemResource
    {
        private bool discrete;
        private RegulatingControlModeKind mode;
        private PhaseCode monitoredPhase;
        private float targetRange;
        private float targetValue;

        // Reference na RegulationSchedule (0..*)
        private List<long> regulationSchedules = new List<long>();

        // Asocijacija na Terminal (0..1)
        private long terminal = 0;

        // Asocijacija na ConductingEquipment (0..*)
        private List<long> regConductingEquipments = new List<long>();

        public RegulatingControl(long globalId)
            : base(globalId)
        {
        }

        public bool Discrete
        {
            get { return discrete; }
            set { discrete = value; }
        }

        public RegulatingControlModeKind Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public PhaseCode MonitoredPhase
        {
            get { return monitoredPhase; }
            set { monitoredPhase = value; }
        }

        public float TargetRange
        {
            get { return targetRange; }
            set { targetRange = value; }
        }

        public float TargetValue
        {
            get { return targetValue; }
            set { targetValue = value; }
        }

        public List<long> RegulationSchedules
        {
            get { return regulationSchedules; }
            set { regulationSchedules = value; }
        }

        public long Terminal
        {
            get { return terminal; }
            set { terminal = value; }
        }

        public List<long> RegConductingEquipments
        {
            get { return regConductingEquipments; }
            set { regConductingEquipments = value; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                RegulatingControl x = (RegulatingControl)obj;
                return (x.discrete == this.discrete &&
                        x.mode == this.mode &&
                        x.monitoredPhase == this.monitoredPhase &&
                        x.targetRange == this.targetRange &&
                        x.targetValue == this.targetValue &&
                        this.regulationSchedules.SequenceEqual(x.regulationSchedules) &&
                        this.terminal == x.terminal &&
                        this.regConductingEquipments.SequenceEqual(x.regConductingEquipments));
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
                case ModelCode.DISCRETE:
                case ModelCode.MODE:
                case ModelCode.MON_PHASE:
                case ModelCode.TARGET_RANGE:
                case ModelCode.TARGET_VALUE:
                case ModelCode.RG_CNTRL_REG_SCHEDULE:
                case ModelCode.RG_CNTRL_TRM:
                case ModelCode.RG_CNTRL_REG_CONEQ:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                case ModelCode.DISCRETE:
                    property.SetValue(discrete);
                    break;
                case ModelCode.MODE:
                    property.SetValue((short)mode);
                    break;
                case ModelCode.MON_PHASE:
                    property.SetValue((short)monitoredPhase);
                    break;
                case ModelCode.TARGET_RANGE:
                    property.SetValue(targetRange);
                    break;
                case ModelCode.TARGET_VALUE:
                    property.SetValue(targetValue);
                    break;
                case ModelCode.RG_CNTRL_REG_SCHEDULE:
                    property.SetValue(regulationSchedules);
                    break;
                case ModelCode.RG_CNTRL_TRM:
                    property.SetValue(terminal);
                    break;
                case ModelCode.RG_CNTRL_REG_CONEQ:
                    property.SetValue(regConductingEquipments);
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
                case ModelCode.DISCRETE:
                    discrete = property.AsBool();
                    break;
                case ModelCode.MODE:
                    mode = (RegulatingControlModeKind)property.AsEnum();
                    break;
                case ModelCode.MON_PHASE:
                    monitoredPhase = (PhaseCode)property.AsEnum();
                    break;
                case ModelCode.TARGET_RANGE:
                    targetRange = property.AsFloat();
                    break;
                case ModelCode.TARGET_VALUE:
                    targetValue = property.AsFloat();
                    break;
                case ModelCode.RG_CNTRL_TRM:
                    terminal = property.AsReference();
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
            if (regulationSchedules != null && regulationSchedules.Count > 0 &&
                (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.RG_CNTRL_REG_SCHEDULE] = regulationSchedules.GetRange(0, regulationSchedules.Count);
            }

            if (terminal != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.RG_CNTRL_TRM] = new List<long> { terminal };
            }

            if (regConductingEquipments != null && regConductingEquipments.Count > 0 &&
                (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.RG_CNTRL_REG_CONEQ] = regConductingEquipments.GetRange(0, regConductingEquipments.Count);
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.REG_SCHEDULE_RG_CNTRL:
                    regulationSchedules.Add(globalId);
                    break;

                case ModelCode.REG_CONEQ_RG_CNTRL:
                    regConductingEquipments.Add(globalId);
                    break;

                case ModelCode.TRM_REG_CNTRL:
                    terminal = globalId;
                    break;

                default:
                    base.AddReference(referenceId, globalId);
                    break;
            }
        }

        public override void RemoveReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.REG_SCHEDULE_RG_CNTRL:
                    if (regulationSchedules.Contains(globalId))
                    {
                        regulationSchedules.Remove(globalId);
                    }
                    else
                    {
                        CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Entity (GID = 0x{0:x16}) doesn't contain reference 0x{1:x16}.", this.GlobalId, globalId);
                    }
                    break;

                case ModelCode.REG_CONEQ_RG_CNTRL:
                    if (regConductingEquipments.Contains(globalId))
                    {
                        regConductingEquipments.Remove(globalId);
                    }
                    else
                    {
                        CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Entity (GID = 0x{0:x16}) doesn't contain reference 0x{1:x16}.", this.GlobalId, globalId);
                    }
                    break;

                case ModelCode.TRM_REG_CNTRL:
                    if (terminal == globalId)
                    {
                        terminal = 0;
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
