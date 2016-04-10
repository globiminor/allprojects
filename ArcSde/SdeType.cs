using System;

namespace ArcSde
{
  /* $Id: sdetype.h,v 1.230 2002/09/27 00:16:11 sdemst83 Exp $ */
  /***********************************************************************
  *
  *N  {sdetype.h}  --  Spatial Database Engine Datatypes/Defines Header File
  *
  *:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
  *
  *P  Purpose:
  * 
  *                  Spatial Database Engine Datatypes/Defines Header File
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
  *E
  ***********************************************************************/

  //-------------------------------------------------------------------------
  /// <summary>
  /// Purpose: Spatial Database Engine Datatypes/Defines Header File. (sdetype.h)
  /// Notes:	 Subset<br/>
  ///		Weitergehende Dokumentationen zu sdetype.h:
  ///		<A href="http://arcsdeonline.esri.com/index.htm">ArcSDE Developer Help, Developer Interface, C API</A><br/>
  /// History: ML 02.09.2003 initial Coding
  /// </summary>
  //-------------------------------------------------------------------------
  public class SdeType
  {

    /************************************************************
    *** ALLOWABLE SHAPE TYPE MASKS FOR LAYERS
    ************************************************************/
    public const Int32 SE_NIL_TYPE_MASK = (Int32)(1L);
    public const Int32 SE_POINT_TYPE_MASK = (Int32)(1L << 1);
    public const Int32 SE_LINE_TYPE_MASK = (Int32)(1L << 2);
    public const Int32 SE_SIMPLE_LINE_TYPE_MASK = (Int32)(1L << 3);
    public const Int32 SE_AREA_TYPE_MASK = (Int32)(1L << 4);

    public const Int32 SE_UNVERIFIED_SHAPE_MASK = (Int32)(1L << 11);
    public const Int32 SE_MULTIPART_TYPE_MASK = (Int32)(1L << 18);

    /************************************************************
    *** ALLOWABLE STORAGE TYPE MASKS FOR LAYERS
    ************************************************************/
    public const Int32 SE_STORAGE_NORMALIZED_TYPE = (Int32)(1L << 23);
    public const Int32 SE_STORAGE_SDEBINARY_TYPE = (Int32)(1L << 24);
    public const Int32 SE_STORAGE_WKB_TYPE = (Int32)(1L << 25);
    public const Int32 SE_STORAGE_SQL_TYPE = (Int32)(1L << 26);
    public const Int32 SE_STORAGE_SPATIAL_TYPE = (Int32)(1L << 27);
    public const Int32 SE_STORAGE_LOB_TYPE = (Int32)(1L << 28);

    /******
	   ACCESS	RIGHTS	CONSTANTS
    **********/

    public const Int32 SE_SELECT_PRIVILEGE = (1 << 1);
    public const Int32 SE_UPDATE_PRIVILEGE = (1 << 2);
    public const Int32 SE_INSERT_PRIVILEGE = (1 << 3);
    public const Int32 SE_DELETE_PRIVILEGE = (1 << 4);

    /************************************************************
    *** ALLOWABLE LAYER CHARATERISTICS
    ************************************************************/
    public const Int32 SE_LAYER_AUTO_REGISTER = (Int32)(1L << 5);
    public const Int32 SE_LAYER_HAS_USER_DEFINED_EXTENT = (Int32)(1L << 6);

    /************************************************************
    *** ALLOWABLE LAYER MASKS
    ************************************************************/
    public const Int32 SE_LAYER_HAS_GEODETIC_EXTENT = (Int32)(1L << 1);


    /************************************************************
    *** OGIS Storage Type Declarations
    ************************************************************/
    public const Int32 SE_OGIS_STORAGE_NORMALIZED = 0;
    public const Int32 SE_OGIS_STORAGE_BINARY = 1;
    public const Int32 SE_OGIS_STORAGE_SQLTYPE = 2;

    /************************************************************
    *** LAYER LOCKING MODES
    ************************************************************/
    public const int SE_WRITE_LOCK = 1;
    public const int SE_READ_LOCK = 2;

    /************************************************************
    *** ATTRIBUTE DATA TYPES
    ************************************************************/
    public const int SE_SMALLINT_TYPE = 1;   /* 2-byte Integer */
    public const int SE_INTEGER_TYPE = 2;   /* 4-byte Integer */
    public const int SE_FLOAT_TYPE = 3;   /* 4-byte Float */
    public const int SE_DOUBLE_TYPE = 4;   /* 8-byte Float */
    public const int SE_STRING_TYPE = 5;   /* Null Term. Character Array */
    public const int SE_BLOB_TYPE = 6;   /* Variable Length Data */
    public const int SE_DATE_TYPE = 7;   /* Struct tm Date */
    public const int SE_SHAPE_TYPE = 8;   /* Shape geometry (SE_SHAPE) */
    public const int SE_RASTER_TYPE = 9;   /* Raster */

    public const int SE_INT16_TYPE = 1; /* 2-byte Integer */
    public const int SE_INT32_TYPE = 2; /* 4-byte Integer */
    public const int SE_FLOAT32_TYPE = 3; /* 4-byte Float */
    public const int SE_FLOAT64_TYPE = 4; /* 8-byte Float */

    public const int SE_XML_TYPE = 10; /* XML Document */
    public const int SE_INT64_TYPE = 11; /* 8-byte Integer */
    public const int SE_UUID_TYPE = 12; /* A Universal Unique ID */
    public const int SE_CLOB_TYPE = 13; /* Character variable length data */
    public const int SE_NSTRING_TYPE = 14; /* UNICODE Null Term. Character Array */
    public const int SE_NCLOB_TYPE = 15; /* UNICODE Character Large Object */

    public const int SE_POINT_TYPE = 20;  /* Point ADT */
    public const int SE_CURVE_TYPE = 21;  /* LineString ADT */
    public const int SE_LINESTRING_TYPE = 22;  /* LineString ADT */
    public const int SE_SURFACE_TYPE = 23;  /* Polygon ADT */
    public const int SE_POLYGON_TYPE = 24;  /* Polygon ADT */
    public const int SE_GEOMETRYCOLLECTION_TYPE = 25;  /* MultiPoint ADT */
    public const int SE_MULTISURFACE_TYPE = 26;  /* LineString ADT */
    public const int SE_MULTICURVE_TYPE = 27;  /* LineString ADT */
    public const int SE_MULTIPOINT_TYPE = 28;  /* MultiPoint ADT */
    public const int SE_MULTILINESTRING_TYPE = 29;  /* MultiLineString ADT */
    public const int SE_MULTIPOLYGON_TYPE = 30;  /* MultiPolygon ADT */
    public const int SE_GEOMETRY_TYPE = 31;  /* Geometry ADT */

    /************************************************************
    *** CONSTANTS DEFINING LIMITS 
    ************************************************************/
    public const int SE_MAX_MESSAGE_LENGTH = 512; /* MAXIMUM ERROR MESSAGE LENGTH */
    public const int SE_MAX_SQL_MESSAGE_LENGTH = 4096; /* MAXIMUM SQL ERROR MESSAGE LENGTH */
    public const int SE_MAX_PATH_LEN = 512; /* MAXIMUM FILE PATH NAME LENGTH */

    public const int SE_MAX_CONFIG_KEYWORD_LEN = 32;  /* MAXIMUM CONFIGURATION KEYWORD 
				LENGTH */
    public const int SE_MAX_CONFIG_STR_LEN = 2048; /* MAXIMUM CONFIGURATION STRING LENGTH */
    public const int SE_MAX_DESCRIPTION_LEN = 64;  /* MAXIMUM LAYER DESCRIPTION LENGTH */
    public const int SE_MAX_FEAT_CLASS_LEN = 128; /* MAXIMUM FILEINFO FEATUE CLASS 
		DESCRIPTION LENGTH */

    public const int SE_MAX_COLUMN_LEN = 32;  /* MAXIMUM COLUMN NAME LENGTH */
    public const int SE_MAX_TABLE_LEN = 160; /* MAXIMUM TABLE NAME LENGTH */
    public const int SE_MAX_SCHEMA_TABLE_LEN = 30;  /* MAXIMUN TABLE 'ONLY' NAME LENGTH */
    public const int SE_MAX_ALIAS_LEN = 32;  /* MAXIMUM TABLE ALIAS LENGTH */
    public const int SE_MAX_ENTITY_LEN = 256; /* MAXIMUM ENTITY TYPE LENGTH */
    public const int SE_MAX_HINT_LEN = 1024;/* MAXIMUM DBMS HINT LENGTH */
    public const int SE_MAX_OWNER_LEN = 32;  /* MAXIMUM TABLE OWNER NAME LENGTH */
    public const int SE_MAX_INDEX_LEN = 160; /* MAXIMUM INDEX NAME LENGTH */
    public const int SE_MAX_COLUMNS = 256; /* MAXIMUM NUMBER OF COLUMNS */
    public const int SE_MAX_ANNO_TEXT_LEN = 255; /* MAXIMUM ANNOTATION TEXT LENGTH */
    public const int SE_MAX_VERSION_LEN = 64;  /* MAXIMUM VERSION NAME LENGTH */
    public const int SE_MAX_VERSION_INPUT_LEN = 62;  /* MAXIMUM USER-SUPPLIED VERSION NAME LENGTH */
    public const int SE_MAX_OBJECT_NAME_LEN = 160; /* MAXIMUM OBJECT NAME LENGTH */
    public const int SE_MAX_METADATA_CLASS_LEN = 32;  /* MAXIMUM CLASS NAME LENGTH */
    public const int SE_MAX_METADATA_PROPERTY_LEN = 32;/* MAXIMUM PROPERTY NAME LENGTH */
    public const int SE_MAX_METADATA_VALUE_LEN = 255; /* MAXIMUM VALUE LENGTH */

    public const int SE_MAX_LOCATOR_PROPERTY_LEN = 32; /* MAXIMUM PROPERTY NAME LENGTH */
    public const int SE_MAX_LOCATOR_VALUE_LEN = 255; /* MAXIMUM VALUE LENGTH */

    public const int SE_MAX_SERVER_LEN = 32;  /* MAXIMUM SERVER NAME LENGTH */
    public const int SE_MAX_INSTANCE_LEN = 32;  /* MAXIMUM INSTANCE NAME LENGTH */
    public const int SE_MAX_PASSWORD_LEN = 32;  /* MAXIMUM PASSWORD NAME LENGTH */
    public const int SE_MAX_DATABASE_LEN = 32;  /* MAXIMUM DATABASE NAME LENGTH */

    public const int SE_MAX_SCL_CODESIZE = 256; /* MAXIMUM SCL OBJECT CODE LENGTH */
    public const int SE_MAX_FUNCTION_LEN = 32;  /* MAXIMUM FUNCTION NAME LENGTH */
    public const int SE_MAX_KEYWORD_LEN = 32;  /* MAXIMUM DBMS RESERVED KEYWORD LENGTH */

    public const int SE_MAX_LOGFILE_NAME_LEN = 64;  /* MAXIMUM BASE LOGFILE NAME LENGTH */

    public const int SE_UUID_STRING_LEN = 40;  /* UUID STRING LENGTH + 1 Nil byte +
		3 bytes for alignment. */


    /* CONSTANTS ALLOWING FOR FULLY QUALIFIED TABLE AND COLUMN NAMES */

    public const int SE_QUALIFIED_TABLE_NAME = (SE_MAX_DATABASE_LEN + SE_MAX_OWNER_LEN +
      SE_MAX_TABLE_LEN + 2);
    public const int SE_QUALIFIED_COLUMN_LEN = (SE_QUALIFIED_TABLE_NAME +
      SE_MAX_COLUMN_LEN + 1);
    public const int SE_QUALIFIED_VERSION_LEN = (SE_MAX_OWNER_LEN + SE_MAX_VERSION_LEN + 1);
    public const int SE_QUALIFIED_LOGFILE_NAME = (SE_MAX_DATABASE_LEN + SE_MAX_OWNER_LEN +
      SE_MAX_LOGFILE_NAME_LEN + 2);
    public const int SE_QUALIFIED_OBJECT_NAME = (SE_MAX_DATABASE_LEN + SE_MAX_OWNER_LEN +
      SE_MAX_OBJECT_NAME_LEN + 2);

    /************************************************************
    *** Minimum gridsize for Layer 
    ************************************************************/
    public const int SE_MIN_GRIDSIZE = 256; /* MINIMUM LAYER GRIDSIZE (system units) */

    /************************************************************
    *** ATTRIBUTE INDICATOR VALUES
    ************************************************************/
    public const int SE_IS_NULL_VALUE = 1;
    public const int SE_IS_NOT_NULL_VALUE = 2;

    /************************************************************
    *** LOG FILE DEFINES
    ************************************************************/
    public const int SE_INPUT_MODE = 0;
    public const int SE_OUTPUT_MODE = 1;
    public const int SE_EXTEND_MODE = 2;
    public const int SE_OUTPUT_NO_DELETE_MODE = 3;

    public const int SE_LOG_PERSISTENT = 1;
    public const int SE_LOG_TEMPORARY = 2;

    public const int SE_LOG_FOR_TABLE = 1;
    public const int SE_LOG_FOR_LAYER = 2;

    public const int SE_LOG_INTERSECT = 1;
    public const int SE_LOG_UNION = 2;
    public const int SE_LOG_DIFFERENCE = 3;
    public const int SE_LOG_SYMDIFF = 4;

    /************************************************************
    *** SPATIAL REFERENCE DEFINES
    ************************************************************/

    public const int SE_MAX_SPATIALREF_AUTHNAME_LEN = 256;
    public const int SE_MAX_SPATIALREF_SRTEXT_LEN = 2048;

    /************************************************************
    *** ALLOWABLE SPATIAL INDEX TYPE DEFINES FOR LAYERS
    *** NOTE: THEIR AVAILABILITY IS DBMS DEPENDENT. 
    ************************************************************/
    public const int SE_SPATIALINDEX_MULTI_GRID = 1;
    public const int SE_SPATIALINDEX_DBTUNE = 0;
    public const int SE_SPATIALINDEX_NONE = -1;
    public const int SE_SPATIALINDEX_RTREE = -2;
    public const int SE_SPATIALINDEX_FIXED_QUADTREE = -3;
    public const int SE_SPATIALINDEX_HYBRID_QUADTREE = -4;
    public const int SE_SPATIALINDEX_UNKNOWN = -5;

    /************************************************************
    *** SEARCH ORDERS
    ************************************************************/
    public const int SE_ATTRIBUTE_FIRST = 1;   /* DO NOT USE SPATIAL INDEX */
    public const int SE_SPATIAL_FIRST = 2;   /* USE SPATIAL INDEX */
    public const int SE_OPTIMIZE = 3;  /* LET SDE DECIDE */

    /************************************************************
    *** QUERY TYPES
    ************************************************************/
    public const int SE_QUERYTYPE_ATTRIBUTE_FIRST = 1;
    public const int SE_QUERYTYPE_JFA = 2;
    public const int SE_QUERYTYPE_JSF = 3;
    public const int SE_QUERYTYPE_JSFA = 4;
    public const int SE_QUERYTYPE_V3 = 5;
    public const int SE_MAX_QUERYTYPE = 5;

    /********************************************************************
    *** SPATIAL FILTER TYPES FOR SPATIAL CONSTRAINTS AND STABLE SEARCHES
    *********************************************************************/
    public const int SE_SHAPE_FILTER = 1;
    public const int SE_ID_FILTER = 2;

    /************************************************************
    *** STABLE OPTIONS
    ************************************************************/
    public const Int32 SE_DELETE_CURRENT = 0;

    /************************************/
    /***  SE_instance_control() options */
    /************************************/
    public const int SE_CONTROL_INSTANCE_SHUTDOWN = 1;
    public const int SE_CONTROL_INSTANCE_PAUSE = 2;
    public const int SE_CONTROL_INSTANCE_RESUME = 3;
    public const int SE_CONTROL_INSTANCE_KILL = 4;
    public const int SE_CONTROL_INSTANCE_KILL_ALL = 5;
    public const int SE_CONTROL_INSTANCE_START = 6;

    /************************************************/
    /***  SDE ROWID pseudo-column -- Obsolete!!!! ***/
    /***  Only present for backwards compatibity: ***/
    /***  Use registered tables row ID's instead. ***/
    /************************************************/
    public const string SE_ROW_ID = "SE_ROW_ID";
  }
}