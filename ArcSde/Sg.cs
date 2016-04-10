using System;

namespace ArcSde
{
	/* $Id: sg.h,v 1.28 2002/11/04 18:02:34 sdemst83 Exp $ */
	/***********************************************************************
	*
	*N  {sg.h} -- Public data structure/prototypes for the shape library.
	*
	*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
	*
	*P  Purpose:
	* 
	*   This file contains the well-known data structures and public 
	*   function prototypes for the shape library. All data structre and
	*   function names are prefixed with Sg to indicate that they are part
	*   of the Shape Geometry (hence, the Sg) library.
	*
	*   Copyright 1992-2002, Environmental Systems Research Institute, Inc.
	*   All rights reserved.  This software is provided with RESTRICTED AND
	*   LIMITED RIGHTS.  Use, duplication, or disclosure by the Government 
	*   is subject to restrictions as set forth in FAR 52.227-14 (JUN 1987)
	*   Alternate III (g) (3) (JUN 1987), FAR 52.227-19 (JUN 1987), or
	*   DFARS 252.227-7013 (c) (1) (ii) (OCT 1988), as applicable.
	*   Contractor/Manufacturer is Environmental Systems Research Institute,
	*   Inc. (ESRI), 380 New York Street., Redlands, California 92373. 
	*   
	*E
	*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
	*
	*H  History:
	*
	*   Scott J. Simon  [11/20/96]  Original coding.
	*E
	***********************************************************************/
	
	//-------------------------------------------------------------------------
	/// <summary>
	/// Purpose: Public data structure/prototypes for the shape library. (Sg.h)
	/// Notes:	 Subset<br/>
	///		Weitergehende Dokumentationen zu Sg.h:
	///		<A href="http://arcsdeonline.esri.com/index.htm">ArcSDE Developer Help, Developer Interface, C API</A><br/>
	/// History: ML 02.09.2003 initial Coding
	/// </summary>
	//-------------------------------------------------------------------------
	public class Sg
	{
		/*
		 * ...Max converted projection string length...
		 */
		public const int SG_MAX_PROJECTION_LEN  = 1024;

		/*
		 * ...Search Methods...
		 */
		public const int     SM_ENVP             = 0;  /* ENVELOPES OVERLAP */
		public const int     SM_ENVP_BY_GRID     = 1;  /* ENVELOPES OVERLAP */
		public const int     SM_CP               = 2;  /* COMMON POINT */
		public const int     SM_LCROSS           = 3;  /* LINE CROSS */
		public const int     SM_COMMON           = 4;  /* COMMON EDGE/LINE */
		public const int     SM_CP_OR_LCROSS     = 5;  /* COMMON POINT OR LINE CROSS */
		public const int     SM_LCROSS_OR_CP     = 5;  /* COMMON POINT OR LINE CROSS */
		public const int     SM_ET_OR_AI         = 6;   /* EDGE TOUCH OR AREA INTERSECT */
		public const int     SM_AI_OR_ET         = 6;   /* EDGE TOUCH OR AREA INTERSECT */
		public const int     SM_ET_OR_II         = 6;   /* EDGE TOUCH OR INTERIOR INTERSECT */
		public const int     SM_II_OR_ET         = 6;   /* EDGE TOUCH OR INTERIOR INTERSECT */
		public const int     SM_AI               = 7;   /* AREA INTERSECT */
		public const int     SM_II               = 7;   /* INTERIOR INTERSECT */
		public const int     SM_AI_NO_ET         = 8;   /* AREA INTERSECT AND NO EDGE TOUCH */
		public const int     SM_II_NO_ET         = 8;   /* INTERIOR INTERSECT AND NO EDGE TOUCH */
		public const int     SM_PC               = 9;   /* PRIMARY CONTAINED IN SECONDARY */
		public const int     SM_SC               = 10;  /* SECONDARY CONTAINED IN PRIMARY */
		public const int     SM_PC_NO_ET         = 11;  /* PRIM CONTAINED AND NO EDGE TOUCH */
		public const int     SM_SC_NO_ET         = 12;  /* SEC CONTAINED AND NO EDGE TOUCH */
		public const int     SM_PIP              = 13;  /* FIRST POINT IN PRIMARY IN SEC */
		public const int     SM_IDENTICAL        = 15;  /* IDENTICAL */
		public const int		 SM_CBM							 = 16;	/* Calculus-based method [Clementini] */

		/*
		 * ...Individual bit-masks for shape relationships...
		 */
		public const Int32     RM_LINE_CROSS           = 1;       /* LINE CROSS */
		public const Int32     RM_COMMON_PT            = (1<<1);  /* COMMON POINT */
		public const Int32     RM_EMBEDDED_PT          = (1<<2);  /* VERTICE EMBEDDED IN LINE */
		public const Int32     RM_CBOUND_SAME          = (1<<3);  /* COM. EDGE SAME DIRECTION */
		public const Int32     RM_CBOUND_DIFF          = (1<<4);  /* COM. EDGE OPP. DIRECTION */
		public const Int32     RM_PARALLEL_OVERLAPPING = (1<<5);  /* PARALLEL OVERLAPPING LINES */
		public const Int32     RM_IDENTICAL            = (1<<6);  /* PRIMARY == SECONDARY */
		public const Int32     RM_AREA_INTERSECT       = (1<<7);  /* AREA INTERSECTION */
		public const Int32     RM_INTERIOR_INTERSECT   = (1<<7);  /* INTERIOR INTERSECTION */
		public const Int32     RM_BOUNDARY_INTERSECT   = (1<<8);  /* BOUNDARY INTERSECTION */
		public const Int32     RM_PRIM_LEP_INTERIOR    = (1<<9);  /* PRIM END PT TOUCHES INTERIOR */
		public const Int32     RM_SEC_LEP_INTERIOR     = (1<<10); /* SEC END PT TOUCHES INTERIOR */
		public const Int32     RM_PRIM_CONTAINED       = (1<<11); /* PRIMARY CONTAINED BY SECONDARY */
		public const Int32     RM_SEC_CONTAINED        = (1<<12); /* SECONDARY CONTAINED BY PRIMARY */
		public const Int32     RM_TESTS_PERFORMED      = (1<<15); /* ANY TESTS PERFORMED? */

		/*
		 * ...Allowable shape types...
		 */
		public const int SG_NIL_SHAPE                = 0;
		public const int SG_POINT_SHAPE              = 1;
		public const int SG_LINE_SHAPE               = 2;
		public const int SG_SIMPLE_LINE_SHAPE        = 4;
		public const int SG_AREA_SHAPE               = 8;
		public const int SG_SHAPE_CLASS_MASK         = 255;  /* Mask all of the previous */
		public const int SG_SHAPE_MULTI_PART_MASK    = 256;  /* Bit flag indicates mult parts */
		public const int SG_MULTI_POINT_SHAPE        = 257;
		public const int SG_MULTI_LINE_SHAPE         = 258;
		public const int SG_MULTI_SIMPLE_LINE_SHAPE  = 260;
		public const int SG_MULTI_AREA_SHAPE         = 264;

		/*
		 * ...Shape's SQL types...
		 */
		public const int SG_UNSPECIFIED_TYPE         = 0;
		public const int SG_POINT_TYPE               = 4;
		public const int SG_POINTM_TYPE              = 5;
		public const int SG_POINTZ_TYPE              = 6;
		public const int SG_POINTZM_TYPE             = 7;
		public const int SG_MULTIPOINT_TYPE          = 8;
		public const int SG_MULTIPOINTM_TYPE         = 9;
		public const int SG_MULTIPOINTZ_TYPE         = 10;
		public const int SG_MULTIPOINTZM_TYPE        = 11;
		public const int SG_LINESTRING_TYPE          = 12;
		public const int SG_LINESTRINGM_TYPE         = 13;
		public const int SG_LINESTRINGZ_TYPE         = 14;
		public const int SG_LINESTRINGZM_TYPE        = 15;
		public const int SG_POLYGON_TYPE             = 16;
		public const int SG_POLYGONM_TYPE            = 17;
		public const int SG_POLYGONZ_TYPE            = 18;
		public const int SG_POLYGONZM_TYPE           = 19;
		public const int SG_MULTILINESTRING_TYPE     = 20;
		public const int SG_MULTILINESTRINGM_TYPE    = 21;
		public const int SG_MULTILINESTRINGZ_TYPE    = 22;
		public const int SG_MULTILINESTRINGZM_TYPE   = 23;
		public const int SG_MULTIPOLYGON_TYPE        = 24;
		public const int SG_MULTIPOLYGONM_TYPE       = 25;
		public const int SG_MULTIPOLYGONZ_TYPE       = 26;
		public const int SG_MULTIPOLYGONZM_TYPE      = 27;
	}
}
