using System;
using System.Collections.Generic;
using CIM.Model;
using FTN.Common;
using FTN.ESI.SIMES.CIM.CIMAdapter.Manager;

namespace FTN.ESI.SIMES.CIM.CIMAdapter.Importer
{
	/// <summary>
	/// PowerTransformerImporter
	/// </summary>
	public class PowerTransformerImporter
	{
		/// <summary> Singleton </summary>
		private static PowerTransformerImporter ptImporter = null;
		private static object singletoneLock = new object();

		private ConcreteModel concreteModel;
		private Delta delta;
		private ImportHelper importHelper;
		private TransformAndLoadReport report;


		#region Properties
		public static PowerTransformerImporter Instance
		{
			get
			{
				if (ptImporter == null)
				{
					lock (singletoneLock)
					{
						if (ptImporter == null)
						{
							ptImporter = new PowerTransformerImporter();
							ptImporter.Reset();
						}
					}
				}
				return ptImporter;
			}
		}

		public Delta NMSDelta
		{
			get 
			{
				return delta;
			}
		}
		#endregion Properties


		public void Reset()
		{
			concreteModel = null;
			delta = new Delta();
			importHelper = new ImportHelper();
			report = null;
		}

		public TransformAndLoadReport CreateNMSDelta(ConcreteModel cimConcreteModel)
		{
			LogManager.Log("Importing PowerTransformer Elements...", LogLevel.Info);
			report = new TransformAndLoadReport();
			concreteModel = cimConcreteModel;
			delta.ClearDeltaOperations();

			if ((concreteModel != null) && (concreteModel.ModelMap != null))
			{
				try
				{
					// convert into DMS elements
					ConvertModelAndPopulateDelta();
				}
				catch (Exception ex)
				{
					string message = string.Format("{0} - ERROR in data import - {1}", DateTime.Now, ex.Message);
					LogManager.Log(message);
					report.Report.AppendLine(ex.Message);
					report.Success = false;
				}
			}
			LogManager.Log("Importing PowerTransformer Elements - END.", LogLevel.Info);
			return report;
		}

		/// <summary>
		/// Method performs conversion of network elements from CIM based concrete model into DMS model.
		/// </summary>
		private void ConvertModelAndPopulateDelta()
		{
			LogManager.Log("Loading elements and creating delta...", LogLevel.Info);

			//// import all concrete model types (DMSType enum)
			ImportDayType();
            ImportTerminal();
            ImportRegulatingControls();
            ImportShuntCompensators();
            ImportSVC();
            ImportRegulationSchedules();
           
            
            
            LogManager.Log("Loading elements and creating delta completed.", LogLevel.Info);
		}

        #region Import

        private void ImportDayType()
        {
            SortedDictionary<string, object> cimDayTYpes = concreteModel.GetAllObjectsOfType("FTN.DayType");
            if (cimDayTYpes != null)
            {
                foreach (KeyValuePair<string, object> cimDayTypePair in cimDayTYpes)
                {
                    FTN.DayType cimDayType = cimDayTypePair.Value as FTN.DayType;

                    ResourceDescription rd = CreateDayType(cimDayType);
                    if (rd != null)
                    {
                        delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
                        report.Report.Append(" ID = ").Append(cimDayType.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
                    }
                    else
                    {
                        report.Report.Append("ID = ").Append(cimDayType.ID).AppendLine(" FAILED to be converted");
                    }
                }
                report.Report.AppendLine();
            }
        }
        private ResourceDescription CreateDayType(FTN.DayType cimDayType)
        {
            ResourceDescription rd = null;
            if (cimDayType != null)
            {
                long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.DAYTYPE, importHelper.CheckOutIndexForDMSType(DMSType.DAYTYPE));
                rd = new ResourceDescription(gid);
                importHelper.DefineIDMapping(cimDayType.ID, gid);

                ////populate ResourceDescription
                ProjekatConverter.PopulateDayTypeProperties(cimDayType, rd, importHelper, report);
            }
            return rd;
        }



        private void ImportTerminal()
		{
			SortedDictionary<string, object> cimTerminals = concreteModel.GetAllObjectsOfType("FTN.Terminal");
			if (cimTerminals != null)
			{
				foreach (KeyValuePair<string, object> cimTerminalPair in cimTerminals)
				{
					FTN.Terminal cimTerminal = cimTerminalPair.Value as FTN.Terminal;

					ResourceDescription rd = CreateTerminal(cimTerminal);
					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append(" ID = ").Append(cimTerminal.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("ID = ").Append(cimTerminal.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateTerminal(FTN.Terminal cimTerminal)
		{
			ResourceDescription rd = null;
			if (cimTerminal != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.TERMINAL, importHelper.CheckOutIndexForDMSType(DMSType.TERMINAL));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimTerminal.ID, gid);

				////populate ResourceDescription
				ProjekatConverter.PopulateTerminalProperties(cimTerminal, rd,importHelper,report);
			}
			return rd;
		}
		
		private void ImportRegulatingControls()
		{
			SortedDictionary<string, object> cimRGs = concreteModel.GetAllObjectsOfType("FTN.RegulatingControl");
			if (cimRGs != null)
			{
				
				foreach (KeyValuePair<string, object> cimRGPair in cimRGs)
				{
					FTN.RegulatingControl cimRG = cimRGPair.Value as FTN.RegulatingControl;

					ResourceDescription rd = CreateRegulatingControlDescription(cimRG);
					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("ID = ").Append(cimRG.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("ID = ").Append(cimRG.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateRegulatingControlDescription(FTN.RegulatingControl cimRG)
		{
			ResourceDescription rd = null;
			if (cimRG != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.REGULATINGCONTROL, importHelper.CheckOutIndexForDMSType(DMSType.REGULATINGCONTROL));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimRG.ID, gid);

				////populate ResourceDescription
				ProjekatConverter.PopulateRegulatingControl(cimRG, rd,importHelper,report);
			}
			return rd;
		}

		private void ImportRegulationSchedules()
		{
			SortedDictionary<string, object> cimRSs = concreteModel.GetAllObjectsOfType("FTN.RegulationSchedule");
			if (cimRSs != null)
			{
				
				foreach (KeyValuePair<string, object> cimRSPair in cimRSs)
				{
                    FTN.RegulationSchedule cimRS= cimRSPair.Value as FTN.RegulationSchedule;

					ResourceDescription rd = CreateRegulationScheduleDescription(cimRS);
					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("ID = ").Append(cimRS.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("ID = ").Append(cimRS.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateRegulationScheduleDescription(FTN.RegulationSchedule cimRS)
		{
			ResourceDescription rd = null;
			if (cimRS != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.REGULATIONSCHEDULE, importHelper.CheckOutIndexForDMSType(DMSType.REGULATIONSCHEDULE));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimRS.ID, gid);

				////populate ResourceDescription
				ProjekatConverter.PopulateRegulationSchedule(cimRS, rd, importHelper, report);
			}
			return rd;
		}

		//StaticVarComepnsator
		private void ImportSVC()
		{
			SortedDictionary<string, object> cimSVCs = concreteModel.GetAllObjectsOfType("FTN.StaticVarCompensator");
			if (cimSVCs != null) 
			{
				foreach (KeyValuePair<string, object> cimSVCPair in cimSVCs)
				{
					FTN.StaticVarCompensator cimSVC = cimSVCPair.Value as FTN.StaticVarCompensator;

					ResourceDescription rd = CreateSVCeDescription(cimSVC);
					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("ID = ").Append(cimSVC.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("ID = ").Append(cimSVC.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateSVCeDescription(FTN.StaticVarCompensator cimSVC)
		{
			ResourceDescription rd = null;
			if (cimSVC != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.STATICVARCOMPENSATOR, importHelper.CheckOutIndexForDMSType(DMSType.STATICVARCOMPENSATOR));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimSVC.ID, gid);

				////populate ResourceDescription
				ProjekatConverter.PopulateStaticVarCompensator(cimSVC, rd, importHelper, report);
			}
			return rd;
		}

		private void ImportShuntCompensators()
		{
			SortedDictionary<string, object> cimSCs = concreteModel.GetAllObjectsOfType("FTN.ShuntCompensator");
			if (cimSCs != null)
			{
				foreach (KeyValuePair<string, object> cimSCPair in cimSCs)
				{
					FTN.ShuntCompensator cimSC = cimSCPair.Value as FTN.ShuntCompensator;

                    ResourceDescription rd = CreateShuntCompensatorDescription(cimSC);
					if (rd != null)
					{
						delta.AddDeltaOperation(DeltaOpType.Insert, rd, true);
						report.Report.Append("ID = ").Append(cimSC.ID).Append(" SUCCESSFULLY converted to GID = ").AppendLine(rd.Id.ToString());
					}
					else
					{
						report.Report.Append("ID = ").Append(cimSC.ID).AppendLine(" FAILED to be converted");
					}
				}
				report.Report.AppendLine();
			}
		}

		private ResourceDescription CreateShuntCompensatorDescription(FTN.ShuntCompensator cimSC)
		{
			ResourceDescription rd = null;
			if (cimSC != null)
			{
				long gid = ModelCodeHelper.CreateGlobalId(0, (short)DMSType.SHUNTCOMPENSATOR, importHelper.CheckOutIndexForDMSType(DMSType.SHUNTCOMPENSATOR));
				rd = new ResourceDescription(gid);
				importHelper.DefineIDMapping(cimSC.ID, gid);

				////populate ResourceDescription
				ProjekatConverter.PopulateShuntCompensator (cimSC, rd, importHelper, report);
			}
			return rd;
		}
		#endregion Import
	}
}

