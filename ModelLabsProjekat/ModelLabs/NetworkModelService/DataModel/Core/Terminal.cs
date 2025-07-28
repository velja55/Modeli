using FTN.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTN.Services.NetworkModelService.DataModel.Core
{
    public class Terminal : IdentifiedObject
    {
        // Asocijacija na ConductingEquipment (0..1)
        private long conductingEquipment = 0;

        // Asocijacija na RegulatingControl (0..1)
        private List<long> regulatingControls=new List<long>();

        public Terminal(long globalId)
            : base(globalId)
        {
        }

        public long ConductingEquipment
        {
            get { return conductingEquipment; }
            set { conductingEquipment = value; }
        }

        public List<long> RegulatingControl
        {
            get { return regulatingControls; }
            set { regulatingControls = value; }
        }

        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                Terminal x = (Terminal)obj;
                return this.conductingEquipment == x.conductingEquipment &&
                       this.regulatingControls == x.regulatingControls;
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
              //  case ModelCode.TRM_CONEQ:
                case ModelCode.TRM_REG_CNTRL:
                    return true;
                default:
                    return base.HasProperty(property);
            }
        }

        public override void GetProperty(Property property)
        {
            switch (property.Id)
            {
                /*case ModelCode.TRM_CONEQ:
                    property.SetValue(conductingEquipment);
                    break;*/
                case ModelCode.TRM_REG_CNTRL:
                    property.SetValue(this.regulatingControls);
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
                /*case ModelCode.TRM_CONEQ:
                    conductingEquipment = property.AsReference();
                    break;*/
                case ModelCode.TRM_REG_CNTRL:
                    regulatingControls = property.AsReferences();
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
          /*  if (conductingEquipment != 0 && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.TRM_CONEQ] = new List<long> { conductingEquipment };
            }*/
            if (regulatingControls != null && (refType == TypeOfReference.Reference || refType == TypeOfReference.Both))
            {
                references[ModelCode.TRM_REG_CNTRL] = regulatingControls;
            }

            base.GetReferences(references, refType);
        }

        public override void AddReference(ModelCode referenceId, long globalId)
        {
            switch (referenceId)
            {
                case ModelCode.CONDEQ_TRM:
                    conductingEquipment = globalId;
                    break;
                case ModelCode.RG_CNTRL_TRM:
                    regulatingControls.Add(globalId);
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
                case ModelCode.CONDEQ_TRM:
                    if (conductingEquipment == globalId)
                    {
                        conductingEquipment = 0;
                    }
                    else
                    {
                        CommonTrace.WriteTrace(CommonTrace.TraceWarning, "Entity (GID = 0x{0:x16}) doesn't contain reference 0x{1:x16}.", this.GlobalId, globalId);
                    }
                    break;
                case ModelCode.RG_CNTRL_TRM:
                    if (regulatingControls.Contains(globalId))
                    {
                        regulatingControls.Remove(globalId);
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
