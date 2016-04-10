

namespace ArcSde
{
	/* $Id: sdeerno.h,v 1.145 2002/10/22 20:45:03 sanj3308 Exp $ */
	/***********************************************************************
	*
	*N  {sdeerno.h}  --  SDE Error Numbers
	*
	*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
	*
	*P  Purpose:
	* 
	*
	*   Copyright 1992-2002, Environmental Systems Research Institute, Inc.
	*   All rights reserved.  This software is provided with RESTRICTED AND
	*   LIMITED RIGHTS.  Use, duplication, or disclosure by the Government 
	*   is subject to restrictions as set forth in FAR 52.227-14 = JUN 1987;
	*   Alternate III = g; = 3; = JUN 1987;, FAR 52.227-19 = JUN 1987;, or
	*   DFARS 252.227-7013 = c; = 1; = ii; = OCT 1988;, as applicable.
	*   Contractor/Manufacturer is Environmental Systems Research Institute,
	*   Inc. = ESRI;, 380 New York Street., Redlands, California 92373. 
	*   
	*E
	*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
	*
	*H  History:
	*
	*      ???                   9/91         Original coding.
	*E
	***********************************************************************/
	
	//-------------------------------------------------------------------------
	/// <summary>
	/// Purpose: SDE Error Numbers (sdeerno.h)
	/// Notes:	 Complete<br/>
	///		Weitergehende Dokumentationen zu sdeerno.h:
	///		<A href="http://arcsdeonline.esri.com/index.htm">ArcSDE Developer Help, Developer Interface, C API</A><br/>
	/// History: ML 02.09.2003 initial Coding
	/// </summary>
	//-------------------------------------------------------------------------
	public class SdeErrNo
	{

		//public const int SDE_ERRNO

		/******************************************************************************
		****    FATAL SDE ERRORS
		******************************************************************************/
		public const int SE_FAILURE              = -1;
		public const int SE_INVALID_LAYERINFO_OBJECT = -2;/* LAYERINFO pointer not initialized. */
		public const int SE_NO_ANNOTATION        = -3;    /* The given shape has no annotation */
		public const int SE_FINISHED             = -4;    /* STREAM LOADING OF SHAPES FINISHED */
		public const int SE_SDE_NOT_STARTED      = -5;    /* SDE NOT STARTED, CANNOT PERFORM FUNCTION */
		public const int SE_UNCHANGED            = -6;    /* THE SPECIFIED SHAPE WAS LEFT UNCHANGED */
		public const int SE_TASKS_EXCEEDED       = -7;    /* THE NUMBER OF SERVER TASKS IS @ MAXIMUM */
		public const int SE_CONNECTIONS_EXCEEDED = -7;    /* THE NUMBER OF SERVER CONNECTIONS IS @ MAXIMUM */
		public const int SE_LOGIN_NOT_ALLOWED    = -8;    /* IOMGR NOT ACCEPTING CONNECTION REQUESTS */
		public const int SE_INVALID_USER         = -9;    /* CANNOT VALIDATE THE SPECIFIED USER AND PASSWORD */
		public const int SE_NET_FAILURE          = -10;   /* NETWORK I/O OPERATION FAILED */
		public const int SE_NET_TIMEOUT          = -11;   /* NETWORK I/O TIMEOUT */
		public const int SE_OUT_OF_SVMEM         = -12;   /* SERVER TASK CANNOT ALLOCATE NEEDED MEMORY */
		public const int SE_OUT_OF_CLMEM         = -13;   /* CLIENT TASK CANNOT ALLOCATE NEEDED MEMORY */
		public const int SE_OUT_OF_CONTEXT       = -14;   /* FUNCTION CALL IS OUT OF CONTEXT */
		public const int SE_NO_ACCESS            = -15;   /* NO ACCESS TO OBJECT */
		public const int SE_TOO_MANY_LAYERS      = -16;   /* Exceeded max_layers in giomgr.defs. */
		public const int SE_NO_LAYER_SPECIFIED   = -17;   /* MISSING LAYER SPECIFICATION */
		public const int SE_LAYER_LOCKED         = -18;   /* SPECIFIED LAYER IS LOCKED */
		public const int SE_LAYER_EXISTS         = -19;   /* SPECIFIED LAYER ALREADY EXISTS */
		public const int SE_LAYER_NOEXIST        = -20;   /* SPECIFIED LAYER DOES NOT EXIST */
		public const int SE_LAYER_INUSE          = -21;   /* SPECIFIED LAYER IS USE BY ANOTHER USER */
		public const int SE_FID_NOEXIST          = -22;   /* SPECIFIED SHAPE = LAYER:FID; DOESN'T EXIST */
		public const int SE_ROW_NOEXIST          = -22;   /* SPECIFIED ROW DOESN'T EXIST */
		public const int SE_FID_EXISTS           = -23;   /* SPECIFIED SHAPE = LAYER:FID; EXISTS */
		public const int SE_ROW_EXISTS           = -23;   /* SPECIFIED ROW EXISTS */
		public const int SE_LAYER_MISMATCH       = -24;   /* Both layers must be the same for this */
		public const int SE_NO_PERMISSIONS       = -25;   /* NO PERMISSION TO PERFORM OPERATION */
		public const int SE_INVALID_NOT_NULL     = -26;   /* COLUMN HAS NOT NULL public constRAINT. */
		public const int SE_INVALID_SHAPE        = -27;   /* INVALID SHAPE, CANNOT BE VERIFIED */
		public const int SE_INVALID_LAYER_NUMBER = -28;   /* MAP LAYER NUMBER OUT OF RANGE */
		public const int SE_INVALID_ENTITY_TYPE  = -29;   /* INVALID ENTITY TYPE */
		public const int SE_INVALID_SEARCH_METHOD = -30;  /* INVALID SEARCH METHOD */
		public const int SE_INVALID_ETYPE_MASK   = -31;   /* INVALID ENTITY TYPE MASK */
		public const int SE_BIND_CONFLICT        = -32;   /* BIND/SET/GET MIS-MATCH */
		public const int SE_INVALID_GRIDSIZE     = -33;   /* INVALID GRID SIZE */
		public const int SE_INVALID_LOCK_MODE    = -34;   /* INVALID LOCK MODE */
		public const int SE_ETYPE_NOT_ALLOWED    = -35;   /* ENTITY TYPE OF SHAPE IS NOT ALLOWED IN LAYER */
		public const int SE_TOO_MANY_POINTS      = -36;   /* Exceeded max points specified. */
		public const int SE_INVALID_NUM_OF_PTS   = -36;   /* Alternate name of above. */
		public const int SE_TABLE_NOEXIST        = -37;   /* DBMS TABLE DOES NOT EXIST */
		public const int SE_ATTR_NOEXIST         = -38;   /* SPECIFIED ATTRIBUTE COLUMN DOESN'T EXIST */
		public const int SE_LICENSE_FAILURE      = -39;   /* Underlying license manager problem. */
		public const int SE_OUT_OF_LICENSES      = -40;   /* No more SDE licenses available. */
		public const int SE_INVALID_COLUMN_VALUE = -41;   /* VALUE EXCEEDS VALID RANGE */
		public const int SE_INVALID_WHERE        = -42;   /* USER SPECIFIED WHERE CLAUSE IS INVALID */
		public const int SE_INVALID_SQL          = -42;   /* USER SPECIFIED SQL CLAUSE IS INVALID */
		public const int SE_LOG_NOEXIST          = -43;   /* SPECIFIED LOG FILE DOES NOT EXIST */
		public const int SE_LOG_NOACCESS         = -44;   /* UNABLE TO ACCESS SPECIFIED LOGFILE */
		public const int SE_LOG_NOTOPEN          = -45;   /* SPECIFIED LOGFILE IS NOT OPEN FOR I/O */
		public const int SE_LOG_IO_ERROR         = -46;   /* I/O ERROR USING LOGFILE */
		public const int SE_NO_SHAPES            = -47;   /* NO SHAPES SELECTED OR USED IN OPERATION */
		public const int SE_NO_LOCKS             = -48;   /* NO LOCKS DEFINED */
		public const int SE_LOCK_CONFLICT        = -49;   /* LOCK REQUEST CONFLICTS W/ ANOTHER ESTABLISHED LOCK */ 
		public const int SE_OUT_OF_LOCKS         = -50;   /* MAXIMUM LOCKS ALLOWED BY SYSTEM ARE IN USE */
		public const int SE_DB_IO_ERROR          = -51;   /* DATABASE LEVEL I/O ERROR OCCURRED */
		public const int SE_STREAM_IN_PROGRESS   = -52;   /* SHAPE/FID STREAM NOT FINISHED, CAN'T EXECUTE */
		public const int SE_INVALID_COLUMN_TYPE  = -53;   /* INVALID COLUMN DATA TYPE */
		public const int SE_TOPO_ERROR           = -54;   /* TOPOLOGICAL INTEGRITY ERROR */
		public const int SE_ATTR_CONV_ERROR      = -55;   /* ATTRIBUTE CONVERSION ERROR */
		public const int SE_INVALID_COLUMN_DEF   = -56;   /* INVALID COLUMN DEFINITION */
		public const int SE_INVALID_SHAPE_BUF_SIZE = -57; /* INVALID SHAPE ARRAY BUFFER SIZE */
		public const int SE_INVALID_ENVELOPE     = -58;   /* ENVELOPE IS NULL, HAS NEGATIVE VALUES OR MIN > MAX 
				*/
		public const int SE_TEMP_IO_ERROR        = -59;   /* TEMP FILE I/O ERROR, CAN'T OPEN OR RAN OUT OF DISK 
				*/
		public const int SE_GSIZE_TOO_SMALL      = -60;   /* SPATIAL INDEX GRID SIZE IS TOO SMALL */
		public const int SE_LICENSE_EXPIRED      = -61;   /* SDE RUN-TIME LICENSE HAS EXPIRED, NO LOGINS 
				ALLOWED */
		public const int SE_TABLE_EXISTS         = -62;   /* DBMS TABLE EXISTS */
		public const int SE_INDEX_EXISTS         = -63;   /* INDEX WITH THE SPECIFIED NAME ALREADY EXISTS */
		public const int SE_INDEX_NOEXIST        = -64;   /* INDEX WITH THE SPECIFIED NAME DOESN'T EXIST */
		public const int SE_INVALID_POINTER      = -65;   /* SPECIFIED POINTER VALUE IS NULL OR INVALID */
		public const int SE_INVALID_PARAM_VALUE  = -66;   /* SPECIFIED PARAMETER VALUE IS INVALID */
		public const int SE_ALL_SLIVERS          = -67;   /* SLIVER FACTOR CAUSED ALL RESULTS TO BE SLIVERS */
		public const int SE_TRANS_IN_PROGRESS    = -68;   /* USER SPECIFIED TRANSACTION IN PROGRESS */
		public const int SE_IOMGR_NO_DBMS_CONNECT = -69;  /* The iomgr has lost its connection
				to the underlying DBMS. */
		public const int SE_DUPLICATE_ARC        = -70;   /* AN ARC = startpt,midpt,endpt; ALREADY EXISTS */
		public const int SE_INVALID_ANNO_OBJECT  = -71;   /* SE_ANNO pointer not initialized. */
		public const int SE_PT_NO_EXIST          = -72;   /* SPECIFIED POINT DOESN'T EXIST IN FEAT */
		public const int SE_PTS_NOT_ADJACENT     = -73;   /* SPECIFIED POINTS MUST BE ADJACENT */
		public const int SE_INVALID_MID_PT       = -74;   /* SPECIFIED MID POINT IS INVALID */
		public const int SE_INVALID_END_PT       = -75;   /* SPECIFIED END POINT IS INVALID */
		public const int SE_INVALID_RADIUS       = -76;   /* SPECIFIED RADIUS IS INVALID */
		public const int SE_LOAD_ONLY_LAYER      = -77;   /* MAP LAYER IS LOAD ONLY MODE, OPERATION NOT ALLOWED 
				*/
		public const int SE_LAYERS_NOT_FOUND     = -78;   /* LAYERS TABLE DOES NOT EXIST. */
		public const int SE_FILE_IO_ERROR        = -79;   /* Error writing or creating an output text file. */
		public const int SE_BLOB_SIZE_TOO_LARGE  = -80;   /* Maximum BLOB size exceeded. */
		public const int SE_CORRIDOR_OUT_OF_BOUNDS = -81; /* Resulting corridor exceeds valid coordinate range 
				*/
		public const int SE_SHAPE_INTEGRITY_ERROR = -82;  /* MODEL INTEGRITY ERROR */
		public const int SE_NOT_IMPLEMENTED_YET  = -83;   /* Function or option is not really
				written yet. */
		public const int SE_CAD_EXISTS           = -84;   /* This shape has a cad. */
		public const int SE_INVALID_TRANSID      = -85;   /* Invalid internal SDE Transaction ID. */
		public const int SE_INVALID_LAYER_NAME   = -86;   /* MAP LAYER NAME MUST NOT BE EMPTY */
		public const int SE_INVALID_LAYER_KEYWORD = -87;  /* Invalid Layer Configuration Keyword used. */
		public const int SE_INVALID_RELEASE      = -88;   /* Invalid Release/Version of SDE server. */
		public const int SE_VERSION_TBL_EXISTS	= -89;	/* VERSION table exists. */

		public const int SE_COLUMN_NOT_BOUND     = -90;    /* Column has not been bound */
		public const int SE_INVALID_INDICATOR_VALUE = -91; /* Indicator variable contains an invalid value */
		public const int SE_INVALID_CONNECTION   = -92;    /* The connection handle is NULL,
				closed or the wrong object. */
		public const int SE_INVALID_DBA_PASSWORD = -93;    /* The DBA password is not correct. */
		public const int SE_PATH_NOT_FOUND       = -94;    /* Coord path not found in shape
				edit op. */
		public const int SE_SDEHOME_NOT_SET      = -95;    /* No SDEHOME environment var set, and we
				need one. */
		public const int SE_NOT_TABLE_OWNER      = -96;    /* User must be table owner. */
		public const int SE_PROCESS_NOT_FOUND    = -97;    /* The process ID specified does not
				correspond on an SDE server.  */
		public const int SE_INVALID_DBMS_LOGIN   = -98;    /* DBMS didn't accept user/password. */
		public const int SE_PASSWORD_TIMEOUT     = -99;    /* Password received was sent > 
				MAXTIMEDIFF seconds before. */
		public const int SE_INVALID_SERVER       = -100;   /* Server machine was not found */
		public const int SE_IOMGR_NOT_AVAILABLE  = -101;   /* IO Mgr task not started on server */
		public const int SE_SERVICE_NOT_FOUND    = -102;   /* No SDE entry in the /etc/services file */
		public const int SE_INVALID_STATS_TYPE   = -103;   /* Tried statisitics on non-numeric */
		public const int SE_INVALID_DISTINCT_TYPE = -104;  /* Distinct stats on invalid type */
		public const int SE_INVALID_GRANT_REVOKE = -105;   /* Invalid use of grant/revoke function */
		public const int SE_INVALID_SDEHOME      = -106;   /* The supplied SDEHOME path is
				invalid or NULL.  */
		public const int SE_INVALID_STREAM       = -107;   /* Stream does not exist */
		public const int SE_TOO_MANY_STREAMS     = -108;   /* Max number of streams exceeded */
		public const int SE_OUT_OF_MUTEXES       = -109;   /* Exceeded system's max number of mutexs. */
		public const int SE_CONNECTION_LOCKED    = -110;   /* This connection is locked to a different
				thread. */
		public const int SE_CONNECTION_IN_USE    = -111;   /* This connection is being used at the
				moment by another thread. */
		public const int SE_NOT_A_SELECT_STATEMENT = -112; /* The SQL statement was not a select */
		public const int SE_FUNCTION_SEQUENCE_ERROR = -113;/* Function called out of sequence */
		public const int SE_WRONG_COLUMN_TYPE    = -114;   /* Get request on wrong column type */
		public const int SE_PTABLE_LOCKED        = -115;   /* This ptable is locked to a different
				thread. */
		public const int SE_PTABLE_IN_USE        = -116;   /* This ptable is being used at the
				moment by another thread. */
		public const int SE_STABLE_LOCKED        = -117;   /* This stable is locked to a different
				thread. */
		public const int SE_STABLE_IN_USE        = -118;   /* This stable is being used at the
				moment by another thread. */
		public const int SE_INVALID_FILTER_TYPE  = -119;   /* Unrecognized spatial filter type. */
		public const int SE_NO_CAD               = -120;   /* The given shape has no CAD. */
		public const int SE_INSTANCE_NOT_AVAILABLE = -121; /* No instance running on server. */
		public const int SE_INSTANCE_TOO_EARLY   = -122;   /* Instance is a version previous to 2.0. */
		public const int SE_INVALID_SYSTEM_UNITS = -123;   /* Systems units < 1 or > 2147483647. */
		public const int SE_INVALID_UNITS        = -124;   /* FEET, METERS, DECIMAL_DEGREES or OTHER. */
		public const int SE_INVALID_CAD_OBJECT   = -125;   /* SE_CAD pointer not initialized. */
		public const int SE_VERSION_NOEXIST      = -126;   /* Version not found. */
		public const int SE_INVALID_SPATIAL_CONSTRAINT = -127; /* Spatial filters invalid for search */
		public const int SE_INVALID_STREAM_TYPE  = -128;   /* Invalid operation for the given stream */
		public const int SE_INVALID_SPATIAL_COLUMN  = -129; /* Column contains NOT NULL values during 
				SE_layer_create= ; */
		public const int SE_NO_SPATIAL_MASKS     = -130;   /* No spatial masks available.  */
		public const int SE_IOMGR_NOT_FOUND      = -131;   /* Iomgr program not found. */
		public const int SE_SYSTEM_IS_CLIENT_ONLY = -132;  /* Operation can not possibly be run on
				this system -- it needs a server. */
		public const int SE_MULTIPLE_SPATIAL_COLS = -133;  /* Only one spatial column allowed */
		public const int SE_INVALID_SHAPE_OBJECT = -134;   /* The given shape object handle is invalid */
		public const int SE_INVALID_PARTNUM      = -135;   /* The specified shape part number does not exist */
		public const int SE_INCOMPATIBLE_SHAPES  = -136;   /* The given shapes are of incompatible types */
		public const int SE_INVALID_PART_OFFSET  = -137;   /* The specified part offset is invalid */
		public const int SE_INCOMPATIBLE_COORDREFS = -138; /* The given coordinate references are incompatible */
		public const int SE_COORD_OUT_OF_BOUNDS  = -139;   /* The specified coordinate exceeds the valid coordinate range */
		public const int SE_LAYER_CACHE_FULL     = -140;   /* Max. Layers exceeded in cache */
		public const int SE_INVALID_COORDREF_OBJECT = -141; /* The given coordinate reference object handle is invalid */
		public const int SE_INVALID_COORDSYS_ID  = -142;   /* The coordinate system identifier is invalid */
		public const int SE_INVALID_COORDSYS_DESC = -143;  /* The coordinate system description is invalid */
		public const int SE_INVALID_ROW_ID_LAYER = -144;   /* SE_ROW_ID owner.table does not match the layer */
		public const int SE_PROJECTION_ERROR     = -145;   /* Error projecting shape points */
		public const int SE_ARRAY_BYTES_EXCEEDED = -146;   /* Max array bytes exceeded */

		public const int SE_POLY_SHELLS_OVERLAP      = -147;  /* 2 donuts or 2 outer shells overlap */
		public const int SE_TOO_FEW_POINTS           = -148;  /* numofpts is less than required for feature */
		public const int SE_INVALID_PART_SEPARATOR   = -149;  /* part separator in the wrong position */
		public const int SE_INVALID_POLYGON_CLOSURE  = -150;  /* polygon does not close properly */
		public const int SE_INVALID_OUTER_SHELL      = -151;  /* A polygon outer shell does not completely
				enclose all donuts for the part */
		public const int SE_ZERO_AREA_POLYGON        = -152;  /* Polygon shell has no area */
		public const int SE_POLYGON_HAS_VERTICAL_LINE = -153; /* Polygon shell contains a vertical line */
		public const int SE_OUTER_SHELLS_OVERLAP     = -154;  /* Multipart area has overlapping parts */
		public const int SE_SELF_INTERSECTING        = -155;  /* Linestring or poly boundary is self-intersecting */
		public const int SE_INVALID_EXPORT_FILE      = -156;  /* Export file is invalid */
		public const int SE_READ_ONLY_SHAPE          = -157;  /* Attempted to modify or free a
				read-only shape from an
													stable. */
		public const int SE_INVALID_DATA_SOURCE      = -158;  /* Invalid data source */
		public const int SE_INVALID_STREAM_SPEC      = -159;  /* Stream Spec parameter exceeds giomgr default */
		public const int SE_INVALID_ALTER_OPERATION  = -160;  /* Tried to remove cad or anno */
		public const int SE_INVALID_SPATIAL_COL_NAME = -161;  /* Spat col name same as table name */
		public const int SE_INVALID_DATABASE         = -162;  /* Invalid database name */
		public const int SE_SPATIAL_SQL_NOT_INSTALLED = -163; /* Spatial SQL extension not
				present in underlying DBMS */
		public const int SE_NORM_DIM_INFO_NOT_FOUND  = -164;  /* Obsolete SDE 3.0.2 error */
		public const int SE_NORM_DIM_TAB_VALUE_NOT_FOUND = -165;  /* Obsolete SDE 3.0.2 error */
		public const int SE_UNSUPPORTED_NORMALIZED_OPERATION = -166;  /* " SDE 3.0.2 error */

		public const int SE_INVALID_REGISTERED_LAYER_OPTION = -167; /* Obsolete SDE 3.0.2 error*/
		public const int SE_READ_ONLY                = -168;  /* Has R/O access to SE_ROW_ID */
		public const int SE_NO_SDE_ROWID_COLUMN      = -169;  /* The current table doesn't have a
				SDE-maintained rowid column. */
		public const int SE_READ_ONLY_COLUMN         = -170;  /* Column is not user-modifiable */
		public const int SE_INVALID_VERSION_NAME     = -171;  /* Illegal or blank version name */
		public const int SE_STATE_NOEXIST            = -172;  /* A specified state is not in
				the VERSION_STATES table. */
		public const int SE_INVALID_STATEINFO_OBJECT = -173;  /* STATEINFO object not
				initialized. */
		public const int SE_VERSION_HAS_MOVED        = -174;  /* Attempted to change version
				state, but already changed. */
		public const int SE_STATE_HAS_CHILDREN       = -175;  /* Tried to open a state which
				has children. */
		public const int SE_PARENT_NOT_CLOSED        = -176;  /* To create a state, the parent
				state must be closed. */
		public const int SE_VERSION_EXISTS           = -177;  /* Version already exists. */
		public const int SE_TABLE_NOT_MULTIVERSION   = -178;  /* Table must be multiversion for
				this operation. */
		public const int SE_STATE_USED_BY_VERSION    = -179;  /* Can't delete state being used by
				a version. */
		public const int SE_INVALID_VERSIONINFO_OBJECT = -180; /* VERSIONINFO object not
				initialized. */
		public const int SE_INVALID_STATE_ID         = -181;  /* State ID out of range or not
				found. */
		public const int SE_SDETRACELOC_NOT_SET      = -182;  /* Environment var SDETRACELOC not
				set to a value   */
		public const int SE_ERROR_LOADING_SSA        = -183;  /* Error loading the SSA  */

		public const int SE_TOO_MANY_STATES          = -184;  /* This operation has more
				states than can fit in SQL. */
		public const int SE_STATES_ARE_SAME          = -185;  /* Function takes 2 <> states,
				but same one was given twice. */
		public const int SE_NO_ROWID_COLUMN          = -186;  /* Table has no usable row ID
				column. */
		public const int SE_NO_STATE_SET             = -187;  /* Call needs state to be set. */

		public const int SE_SSA_FUNCTION_ERROR       = -188;  /* Error executing SSA function */

		public const int SE_INVALID_REGINFO_OBJECT   = -189;  /* REGINFO object !initialized. */
		public const int SE_NO_COMMON_LINEAGE        = -190;  /* Attempting to trim between
				states on diff. branches */
		public const int SE_STATE_INUSE              = -191;  /* State is being modified. */
		public const int SE_STATE_TREE_INUSE         = -192;  /* Try to lock tree, and some
				state in tree already locked. */
		public const int SE_INVALID_RASTER_COLUMN    = -193;  /* Raster column has non-NULL values
				or used as row_id column */
		public const int SE_RASTERCOLUMN_EXISTS      = -194;  /* Raster column already exists */
		public const int SE_INVALID_MVTABLE_INDEX    = -195;  /* Unique indexes are not allowed
				on multiversion tables. */
		public const int SE_INVALID_STORAGE_TYPE     = -196;  /* Invalid layer storage type. */

		public const int SE_AMBIGUOUS_NIL_SHAPE      = -197;  /* No SQL type provided when 
				converting NIL shape to text */
		public const int SE_INVALID_BYTE_ORDER       = -198;  /* Invalid byte order for 
				Well-Known Binary shape */
		public const int SE_INVALID_GEOMETRY_TYPE    = -199;  /* Shape type in the given shape
				is not a valid geometry type */
		public const int SE_INVALID_NUM_MEASURES     = -200;  /* Number of measures in shape must
				be zero or equal to number 
													of points */
		public const int SE_INVALID_NUM_PARTS        = -201;  /* Number of parts in shape
				is incorrect for its
													geometry type */
		public const int SE_BINARY_TOO_SMALL         = -202;  /* Memory allocated for ESRI
				binary shape is too small */
		public const int SE_SHAPE_TEXT_TOO_LONG      = -203;  /* Shape text exceeds the 
				supplied maximum string length */
		public const int SE_SHAPE_TEXT_ERROR         = -204;  /* Found syntax error in the 
				supplied shape text */
		public const int SE_TOO_MANY_PARTS           = -205;  /* Number of parts in shape is more
				than expected for the given
														shape text */
		public const int SE_TYPE_MISMATCH            = -206;  /* Shape's SQL type is not as 
				expected when public constructing 
												shape from text */
		public const int SE_SQL_PARENTHESIS_MISMATCH = -207;  /* Found parentheses mismatch 
				when parsing shape text */
		public const int SE_NIL_SHAPE_NOT_ALLOWED    = -208;  /* NIL shape is not allowed for
				Well-Known Binary 
							represenation */
		public const int SE_INSTANCE_ALREADY_RUNNING = -209;  /* Tried to start already running
				SDE instance. */
		public const int SE_UNSUPPORTED_OPERATION    = -210;  /* The operation requested is 
				unsupported. */
		public const int SE_INVALID_EXTERNAL_LAYER_OPTION = -211;  /* Invalid External layer
				option. */ 
		public const int SE_NORMALIZE_VALUE_NOT_FOUND = -212; /* Normalized layer dimension 
				table value not found. */
		public const int SE_INVALID_QUERY_TYPE       = -213;  /* Invalid query type. */
		public const int SE_NO_TRACE_LIBRARY         = -214;  /* No trace functions in this
				library */
		public const int SE_TRACE_ON                 = -215;  /* Tried to enable tracing that
				was already on */ 
		public const int SE_TRACE_OFF                = -216;  /* Tried to disable tracing that
				was already off */ 
		public const int SE_SCL_SYNTAX_ERROR         = -217;  /* SCL Compiler doesn't like the
				SCL stmt */
		public const int SE_TABLE_REGISTERED         = -218;  /* Table already registered. */
		public const int SE_INVALID_REGISTRATION_ID  = -219;  /* Registration ID out of range */
		public const int SE_TABLE_NOREGISTERED       = -220;  /* Table not registered. */
		public const int SE_TOO_MANY_REGISTRATIONS   = -221;  /* Exceeded max_registrations. */
		public const int SE_DELETE_NOT_ALLOWED       = -222;  /* This object can not be deleted,
				other objects depend on it. */
		public const int SE_ROWLOCKING_ENABLED       = -223;  /* Row locking enabled      */
		public const int SE_ROWLOCKING_NOT_ENABLED   = -224;  /* Row locking not enabled  */
		public const int SE_RASTERCOLUMN_INUSE       = -225;  /* Specified raster column is used
				by another user */
		public const int SE_RASTERCOLUMN_NOEXIST     = -226;  /* The specified raster column 
				does not exist */
		public const int SE_INVALID_RASTERCOLUMN_NUMBER = -227; /* Raster column number 
				out of range */
		public const int SE_TOO_MANY_RASTERCOLUMNS   = -228;  /* Maximum raster column 
				number exceeded */
		public const int SE_INVALID_RASTER_NUMBER    = -229;  /* Raster number out of range */
		public const int SE_NO_REQUEST_STATUS        = -230;  /* cannot determine 
				request status */
		public const int SE_NO_REQUEST_RESULTS       = -231;  /* cannot open request results */ 
		public const int SE_RASTERBAND_EXISTS        = -232;  /* Raster band already exists */
		public const int SE_RASTERBAND_NOEXIST       = -233;  /* The specified raster band 
				does not exist */
		public const int SE_RASTER_EXISTS            = -234;  /* Raster already exists */
		public const int SE_RASTER_NOEXIST           = -235;  /* The specified raster 
				does not exist */
		public const int SE_TOO_MANY_RASTERBANDS     = -236;  /* Maximum raster band 
				number exceeded */ 
		public const int SE_TOO_MANY_RASTERS         = -237;  /* Maximum raster number 
				exceeded */ 
		public const int SE_VIEW_EXISTS              = -238;   /* DBMS VIEW EXISTS */
		public const int SE_VIEW_NOEXIST             = -239;   /* DBMS VIEW NOT EXISTS */
		public const int SE_LOCK_EXISTS              = -240;   /* Lock record exist */
		public const int SE_ROWLOCK_MASK_CONFLICT    = -241;   /* Rowlocking mask conflict */
		public const int SE_NOT_IN_RASTER            = -242;  /* Raster band specified 
				not in a raster */
		public const int SE_INVALID_RASBANDINFO_OBJECT = -243; /* RASBANDINFO object
				not initialized */
		public const int SE_INVALID_RASCOLINFO_OBJECT = -244; /* RASCOLINFO object
				not initialized */
		public const int SE_INVALID_RASTERINFO_OBJECT = -245; /* RASTERINFO object 
				not initialized */
		public const int SE_INVALID_RASTERBAND_NUMBER = -246; /* Raster band number
				out of range */
		public const int SE_MULTIPLE_RASTER_COLS      = -247; /* Only one raster column allowed */
		public const int SE_TABLE_SCHEMA_IS_LOCKED    = -248; /* Table is being locked already */
		public const int SE_INVALID_LOGINFO_OBJECT    = -249; /* SE_LOGINFO pointer not initialized. */
		public const int SE_SQL_TOO_LONG              = -250; /* Operation generated a SQL
				statement too big to process.*/
		public const int SE_UNSUPPORTED_ON_VIEW       = -251; /* Not supported on a View.*/
		public const int SE_LOG_EXISTS                = -252; /* Specified log file exists already. */
		public const int SE_LOG_IS_OPEN               = -253; /* Specified log file is open. */
		public const int SE_SPATIALREF_EXISTS         = -254; /* Spatial reference entry exists. */
		public const int SE_SPATIALREF_NOEXIST        = -255; /* Spatial reference entry does not exist. */
		public const int SE_SPATIALREF_IN_USE         = -256; /* Spatial reference entry is
				in use by one or more layers. */
		public const int SE_INVALID_SPATIALREFINFO_OBJECT = -257; /* The SE_SPATIALREFINFO object
				is not initialized. */
		public const int SE_SEQUENCENBR_EXISTS        = -258; /* Raster band sequence number 
				already exits. */
		public const int SE_INVALID_QUERYINFO_OBJECT  = -259; /* SE_QUERYINFO pointer not initialized. */
		public const int SE_QUERYINFO_NOT_PREPARED    = -260; /* SE_QUERYINFO pointer is not prepared for query. */
		public const int SE_INVALID_RASTILEINFO_OBJECT   = -261; /* RASTILEINFO object not 
				initialized */
		public const int SE_INVALID_RASCONSTRAINT_OBJECT = -262; /* SE_RASpublic constRAINT object not 
				initialized */
		public const int SE_INVALID_METADATA_RECORD_ID = -263;  /* invalid record id number */
		public const int SE_INVALID_METADATA_OBJECT = -264;   /* SE_METADATAINFO pointer not 
				initialized */
		public const int SE_INVALID_METADATA_OBJECT_TYPE = -265; /* unsupported object type */
		public const int SE_SDEMETADATA_NOT_FOUND        = -266; /* SDEMETADATA table does not exist */
		public const int SE_METADATA_RECORD_NOEXIST      = -267; /* Metadata record does not exist. */
		public const int SE_GEOMETRYCOL_NOEXIST          = -268; /* Geometry entry does not exist */
		public const int SE_INVALID_FILE_PATH            = -269; /* File path too long or invalid */
		public const int SE_INVALID_LOCATOR_OBJECT_TYPE  = -270; /* Locator object not initialized */
		public const int SE_INVALID_LOCATOR              = -271; /* Locator cannot be validated */
		public const int SE_TABLE_HAS_NO_LOCATOR         = -272; /* Table has no associated locator */
		public const int SE_INVALID_LOCATOR_CATEGORY     = -273; /* Locator cateogry is not specified */
		public const int SE_INVALID_LOCATOR_NAME         = -274; /* Invalid locator name */
		public const int SE_LOCATOR_NOEXIST              = -275; /* Locator does not exist */
		public const int SE_LOCATOR_EXISTS               = -276; /* A locator with that name exists */
		public const int SE_INVALID_LOCATOR_TYPE         = -277; /* Unsupported Locator type */
		public const int SE_NO_COORDREF                  = -278; /* No coordref defined */
		public const int SE_CANT_TRIM_RECONCILED_STATE   = -279; /* Can't trim past a reconciled
				state. */
		public const int SE_FILE_OBJECT_NOEXIST          = -280; /* Fileinfo object does not 
				exist. */
		public const int SE_FILE_OBJECT_EXISTS           = -281; /* Fileinfo object already 
				exists. */
		public const int SE_INVALID_FILEINFO_OBJECT      = -282; /* Fileinfo object not 
				initialized. */
		public const int SE_INVALID_FILEINFO_OBJECT_TYPE = -283; /* Unsupported Fileinfo object 
				type. */
		public const int SE_RASTERBAND_NO_STATS          = -284; /* No statistics available for
				this raster band. */
		public const int SE_VERSION_HAS_CHILDREN         = -285; /* Can't delete a version with
				children. */
		                                                  
		public const int SE_SQLTYPE_UNSUPPORTED_ETYPE    = -286; /* SQL type does not support 
				ANNO or CAD at current release */
		public const int SE_NO_DBTUNE_FILE               = -287; /* The DBTUNE file is missing 
				or unreadable. */
		public const int SE_LOG_SYSTABLES_CREATE_FAILED  = -288; /* Logfile system tables do not
				exist. */
		public const int SE_OBJECT_RESTRICTED            = -289; /* This app can't perform this
				operation on this object. */
		public const int SE_INVALID_GEOGTRAN_OBJECT      = -290; /* The given geographic 
				transformation object 
												handle is invalid */
		public const int SE_COLUMN_EXISTS                = -291; /* Column already exists */
		public const int SE_SQL_KEYWORD                  = -292; /* SQL keyword violation. */
		public const int SE_INVALID_OBJECTLOCKINFO_OBJECT = -293; /* The supplied objectlock
				handle is bad. */
		public const int SE_RASTERBUFFER_TOO_SMALL       = -294; /* The raster buffer size 
				is too small. */
		public const int SE_INVALID_RASTER_DATA          = -295; /* Invalid raster data */
		public const int SE_OPERATION_NOT_ALLOWED        = -296; /* This operation is not 
				allowed */
		public const int SE_INVALID_RASTERATTR_OBJECT    = -297; /* SE_RASTERATTR object not 
				initialized */
		public const int SE_INVALID_VERSION_ID           = -298; /* Version ID out of range. */
		public const int SE_MVTABLE_CANT_BE_LOAD_ONLY    = -299; /* Attempting to make an MV
				table load-only */
		public const int SE_INVALID_SDO_GEOM_METADATA_OBJ = -300; /* The user-supplied table/
				column is invalid. */
		public const int SE_ROW_OUT_OF_SEQUENCE          = -301; /* The next row was not the row
				expected. */
		public const int SE_INSTANCE_IS_READ_ONLY        = -302; /* The ArcSDE instance is 
				read-only */
		public const int SE_MOSAIC_NOT_ALLOWED           = -303; /* Image mosaicking is not
				allowed */
		public const int SE_INVALID_RASTER_BITMAP        = -304; /* Invalid raster bitmap 
				object */
		public const int SE_SEQUENCENBR_NOEXIST          = -305; /* The specified band sequence 
				number does not exist. */
		public const int SE_SQLTYPE_INVALID_FEATURE_TYPE = -306; /* Invalid SQLTYPE feature type
				= i.e. Rect, Arc, Circle; */ 
		public const int SE_DBMS_OBJECTS_NOT_SUPPORTED   = -307; /* DBMS Objects = Spatial, ADT's
				not supported */ 
		public const int SE_BINARY_CONV_NO_COLUMNS_FOUND = -308; /* No columns found for binary
				conversion = LOB/LONGRAW; */
		public const int SE_RASTERBAND_NO_COLORMAP       = -309; /* The raster band has no
				colormap. */
		public const int SE_INVALID_BIN_FUNCTION         = -310; /* Invalid raster band bin 
				function. */
		public const int SE_INVALID_RASTERBAND_STATS     = -311; /* Invalid raster band
				statistics. */
		public const int SE_INVALID_RASTERBAND_COLORMAP  = -312; /* Invalid raster band 
				colormap */
		public const int SE_INVALID_RASTER_KEYWORD       = -313; /* Invalid raster layer
				configuration keyword */
		public const int SE_INCOMPATIBLE_INSTANCE        = -314; /* This sort of iomgr can't run
				on this sort of instance. */
		public const int SE_INVALID_VOLUME_INFO          = -315; /* Export file's volume info is
				invalid */
		public const int SE_INVALID_COMPRESSION_TYPE     = -316; /* Invalid compression type */
		public const int SE_INVALID_INDEX_PARAM          = -317; /* Invalid index parameter */
		public const int SE_INVALID_INDEX_TYPE           = -318; /* Invalid index type */
		public const int SE_SET_VALUE_CONFLICT           = -319; /* Try to set conflicting value 
				in object */
		public const int SE_ADT_DATATYPE_NOT_SUPPORTED   = -320; /* Abstract Data types not
				supported */ 
		public const int SE_NO_SPATIAL_INDEX             = -321; /* No spatial index */
		public const int SE_INVALID_IDENTIFIER           = -322; /* Name not valid for DBMS */
		public const int SE_REGISTERED_TABLE_ROWID_EXIST = -323; /* ROWID for Oracle Spatial
				table already exists. */
		public const int SE_SERVER_LIB_LOAD_ERROR        = -324; /* gsrvr dll for direct could
				not be loaded. */
		public const int SE_REGISTRATION_NOT_ALLOWED     = -325; /* The table can not be
				registered. */
		public const int SE_UNSUPPORTED_ON_MVTABLE       = -326; /* Operation not supported on
				multiversion table. */
		public const int SE_NO_ARCSDE_LICENSE            = -327; /* No ArcSDE server license found */
		public const int SE_UNSUPPORTED_EXPORT_FILE      = -328; /* Exportfile is not supported */
		public const int SE_TABLE_INUSE                  = -329; /* Specified table is in use  */

		public const int SE_DB_SRCH_OUTGEOD_EXTENT       = -369;  /* Search window completely 
				outside oracle spatial geodetic extent */

		/******************************************************************************
		****    NON-FATAL SDE WARNINGS
		******************************************************************************/

		public const int SE_SUCCESS               = 0;
		public const int SE_SDE_WARNING           = -1000;  /* BASE NUMBER FOR WARNING CODES */
		public const int SE_ETYPE_CHANGED         = -1001;  /* FUNCTION CHANGED ENTITY TYPE OF 
				FEAT */
		public const int SE_NO_ROWS_DELETED       = -1002;  /* No rows were deleted. */
		public const int SE_TOO_MANY_DISTINCTS    = -1003;  /* Too many distinct values 
				in stats */
		public const int SE_NULL_VALUE            = -1004;  /* Request column value is NULL */
		public const int SE_NO_ROWS_UPDATED       = -1005;  /* No rows were updated */
		public const int SE_NO_CPGCVT             = -1006;  /* No codepage conversion  */
		public const int SE_NO_CPGHOME            = -1007;  /* Cannot find codepage directory */
		public const int SE_DBMS_DOES_NOT_SUPPORT = -1008;  /* DBMS does NOT support 
				this function */
		public const int SE_INVALID_FUNCTION_ID   = -1009;  /* Invalid DBMS function id */
		public const int SE_LAYERS_UPDATE_FAILED  = -1010;  /* Update layer extent failed */
		public const int SE_NO_LOCALIZED_MESSAGE  = -1011;  /* No localized error messages */
		public const int SE_SPATIAL_INDEX_NOT_CREATED = -1012; /* Spatial index not created,
				server inability to support
													SPIDX_PARAM specified */


		/******************************************************************************
		****    SDE Exit Codes
		****
		****      These are the status codes found in giomgr.log when a connection's
		****    gsrvr exits = as opposed to going down on a signal or with an exception
		****    code -- those are documented in the operating system's documentation;.
		******************************************************************************/

		public const int SE_EXIT_SUCCESS                 = 0;
		public const int SE_EXIT_INVALID_COMMAND_LINE    = 1;
		public const int SE_EXIT_IOMGR_IO_ERROR          = 2;
		public const int SE_EXIT_DBMS_CONNECT_REJECTED   = 3;
		public const int SE_EXIT_SHARED_MEMORY_ERROR     = 4;
		public const int SE_EXIT_MALLOC_ERROR            = 5;
		public const int SE_EXIT_CLIENT_IO_ERROR         = 6;
		public const int SE_EXIT_LOGFILE_INIT_FAILED     = 7;
		public const int SE_EXIT_LOST_CLIENT             = 8;
		public const int SE_EXIT_BAD_COMMAND_CODE        = 9;
		public const int SE_EXIT_REGISTRY_INIT_FAILED   = 10;
		public const int SE_EXIT_LAYERS_INIT_FAILED     = 11;
		public const int SE_EXIT_METADATA_INIT_FAILED   = 12;
		public const int SE_EXIT_RASTER_INIT_FAILED     = 13;
		public const int SE_EXIT_VERSION_INIT_FAILED    = 14;
		public const int SE_EXIT_LOCATOR_INIT_FAILED    = 15;
		public const int SE_EXIT_NO_LOCALIZED_MESSAGE   = 16;
		public const int SE_EXIT_MUTEX_ERROR            = 17;

		//	#endif /* SDE_ERRNO */
	}
}
