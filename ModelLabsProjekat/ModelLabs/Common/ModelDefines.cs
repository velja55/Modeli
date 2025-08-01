using System;
using System.Collections.Generic;
using System.Text;

namespace FTN.Common
{
	
	public enum DMSType : short
	{		
		MASK_TYPE = unchecked((short)0xFFFF),

       
        TERMINAL = 0x0001,
        REGULATIONSCHEDULE = 0x0002,
        REGULATINGCONTROL = 0x0003,
        STATICVARCOMPENSATOR = 0x0004,
        SHUNTCOMPENSATOR = 0x0005,
        DAYTYPE = 0x0006,


    }

    [Flags]
	public enum ModelCode : long
	{
        IDOBJ = 0x1000000000000000,
        IDOBJ_GID = 0x1000000000000104,
        IDOBJ_ALIAS = 0x1000000000000207,
        IDOBJ_MRID = 0x1000000000000307,
        IDOBJ_NAME = 0x1000000000000407,

        RG_CNTRL = 0x1120000000030000,
        DISCRETE = 0x1120000000030101,
        MODE = 0x112000000003020a,
        MON_PHASE = 0x112000000003030a,
        TARGET_RANGE = 0x1120000000030405,
        TARGET_VALUE = 0x1120000000030505,
        RG_CNTRL_REG_SCHEDULE = 0x1120000000030619,
        RG_CNTRL_REG_CONEQ = 0x1120000000030719,
        RG_CNTRL_TRM = 0x1120000000030809,

        REG_CONEQ = 0x1111100000000000,
        REG_CONEQ_RG_CNTRL = 0x1111100000000109,

        STAT_VAR_COMP = 0x1111100000040000,

        SHUNT_COMP = 0x1111100000050000,

        REG_SCHEDULE = 0x1211100000020000,
        REG_SCHEDULE_RG_CNTRL = 0x1211100000020109,

        DAYTYPE = 0x1400000000060000,
        DAYTYPE_SEASON_DAYTYPE = 0x1400000000060119,

        SEASON_DAYTYPE = 0x1211000000000000,
        SEASON_DAYTYPE_DAYTYPE = 0x1211000000000109,

        PSR = 0x1100000000000000,

        TRM = 0x1300000000010000,
        //TRM_CONEQ = 0x1300000000010109,
        TRM_REG_CNTRL = 0x1300000000010219,

        BIS = 0x1200000000000000,
        BIS_STARTTIME = 0x1200000000000108,
        BIS_VAL1MUL = 0x120000000000020a,
        BIS_VAL1UNIT = 0x120000000000030a,
        BIS_VAL2MUL = 0x120000000000040a,
        BIS_VAL2UNIT = 0x120000000000050a,

        EQUIPMENT = 0x1110000000000000,
        EQUIPMENT_AGGREGATE = 0x1110000000000101,
        EQUIPMENT_NORMALLYINSERVICE = 0x1110000000000201,

        CONDEQ = 0x1111000000000000,
        CONDEQ_TRM = 0x1111000000000119,

        RIS = 0x1210000000000000

    }

    [Flags]
	public enum ModelCodeMask : long
	{
		MASK_TYPE			 = 0x00000000ffff0000,
		MASK_ATTRIBUTE_INDEX = 0x000000000000ff00,
		MASK_ATTRIBUTE_TYPE	 = 0x00000000000000ff,

		MASK_INHERITANCE_ONLY = unchecked((long)0xffffffff00000000),
		MASK_FIRSTNBL		  = unchecked((long)0xf000000000000000),
		MASK_DELFROMNBL8	  = unchecked((long)0xfffffff000000000),		
	}																		
}


