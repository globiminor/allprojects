using System;
using System.Runtime.InteropServices;

namespace ArcSde
{
  //-------------------------------------------------------------------------
  /// <summary>
  /// Purpose: Bibiliothek aller benötigten Funktionen der ArcSDE 9.0 C-API.
  ///		Die statischen Methoden erlauben "native" Zugriff aus .net mittels
  ///		Platform Invokation.<br/>
  /// Notes:	Die in ArcSde generierten Fehlerangaben werden als "Return
  ///		Code" in den managed Code übermittelt
  ///		Weitergehende Dokumentationen zu allen Methoden und Strukturen von 
  ///		LayoutLibrary: <A href="http://arcsdeonline.esri.com/index.htm">ArcSDE 
  ///		Developer Help, Developer Interface, C API</A><br/>
  /// History: ML 03.09.2003 initial coding
  /// </summary>
  //-------------------------------------------------------------------------
  internal class CApi
  {
    private const string sdeDll = "sde.dll";

    #region instance
    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_instance_get_users(string server, string instance,
      ref IntPtr userList, ref int userCount);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_instance_free_users(IntPtr userList, int userCount);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_instance_control(string server, string instance, string passwd,
      int option, int pid);

    #endregion

    #region connection

    #region state
    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_state_get_info(IntPtr connection, Int32 stateId, IntPtr stateInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_state_create(IntPtr connection,
        IntPtr stateInfo, Int32 parentStateId, IntPtr resultStateInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_state_new_edit(IntPtr connection,
        IntPtr stateInfo, Int32 parentStateId, IntPtr resultStateInfo);

    #endregion

    #region version
    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_version_get_info(IntPtr connection, string versionName, IntPtr versionInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_version_change_state(IntPtr connection, IntPtr versionInfo, Int32 newStateId);
    #endregion

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_connection_create(string server,
        string instance,
        string database,
        string username,
        string password,
        ref Se_Error error,
        ref IntPtr conn);
    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_connection_reconnect(
      IntPtr conn,ref Se_Error error);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_connection_start_transaction(IntPtr conn);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_connection_rollback_transaction(IntPtr conn);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_connection_commit_transaction(IntPtr conn);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_connection_get_ext_error(
        IntPtr Connection,
        ref Se_Error Error);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_connection_free(IntPtr conn);

    #endregion

    #region coordref

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_coordref_create(out IntPtr coordref);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_coordref_duplicate(IntPtr src_ref, IntPtr tgt_ref);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_coordref_set_xy(IntPtr coordref,
        double falsex,
        double falsey,
        double xyunits);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_coordref_free(IntPtr coordref);
    #endregion

    #region layerinfo

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layerinfo_create(IntPtr coordref,
        out IntPtr layerinfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layerinfo_set_spatial_column(
        IntPtr layerinfo,
        string table,
        string column);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layer_get_info(IntPtr Connection,
        string table,
        string column,
        IntPtr Layer);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layer_get_info_list(IntPtr connection,
      out IntPtr layer_list, // SE_LAYERINFO** layer_list,
      out int num_layers);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_layer_free_info_list(int count, IntPtr layer_list);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layer_get_info_by_id(IntPtr Connection,
        Int32 layer_id,
        IntPtr Layer);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layerinfo_get_spatial_column(IntPtr Layerinfo,
        [In, Out] char[] table,
        [In, Out] char[] column);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layerinfo_get_coordref(IntPtr layerinfo,
        IntPtr Coordref);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layerinfo_get_envelope(IntPtr layerinfo,
        ref Se_Envelope extent);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layerinfo_set_shape_types(
        IntPtr layerinfo,
        Int32 shape_types);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layerinfo_get_shape_types(
        IntPtr layerinfo,
        out Int32 shape_types);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layerinfo_set_grid_sizes(IntPtr layerinfo,
        double grid_size,
        double grid_size2,
        double grid_size3);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layerinfo_get_grid_sizes(IntPtr layerinfo,
        out double grid_size,
        out double grid_size2,
        out double grid_size3);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layerinfo_set_creation_keyword(
        IntPtr layerinfo,
        string config_keyword);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_layerinfo_free(IntPtr Layerinfo);
    #endregion

    #region layer

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_layer_create(IntPtr Connection,
        IntPtr Layerinfo,
        Single initialf,
        Single avg_points);
    #endregion

    #region error

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_error_get_string(Int32 error_code,
        [Out] char[] error_string);
    #endregion

    #region query
    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_queryinfo_create(ref IntPtr queryInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_queryinfo_free(IntPtr queryInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_queryinfo_set_tables(IntPtr queryInfo,
        int nrTables, string[] tableList, string[] aliasList);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_queryinfo_set_columns(IntPtr queryInfo,
        int nrColumns, string[] columnList);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_queryinfo_set_where_clause(IntPtr queryInfo,
        string whereClause);

    #endregion

    #region raster
    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasterinfo_create(ref IntPtr rasterinfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_rasterinfo_free(IntPtr rasterinfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasterattr_create(ref IntPtr rasAttr, bool inputMode);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_rasterattr_free(IntPtr rasAttr);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasterattr_get_extent_by_level(IntPtr rasterAttr,
        ref Se_Envelope extent, ref double coordOffsetX, ref double coordOffsetY, int level);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasterattr_get_image_size_by_level(IntPtr rasterAttr,
        ref int pixelWidth, ref int pixelHeight, ref int pixelOffetX, ref int pixelOffsetY,
        ref int nrBands, int level);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasterattr_get_tile_size(IntPtr rasterAttr, ref int tileWidth, ref int tileHeight);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rascolinfo_create(ref IntPtr rasterColumInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_rascolinfo_free(IntPtr rasterColumnInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rascolinfo_get_id(IntPtr rasterColumnInfo,
        ref Int32 rasterColumnId);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rascolinfo_get_raster_column(IntPtr rasColInfo,
      char[] table, char[] column);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    unsafe
    internal static extern Int32 SE_rastercolumn_get_info_list(IntPtr connection,
      out IntPtr* arrayPos, out int count);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rastercolumn_get_info_by_name(IntPtr connection,
            string table, string column, IntPtr rasColInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_raster_get_info_by_id(IntPtr connection,
        int columnId, int rasterId, IntPtr rasterBandInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_raster_get_bands(IntPtr connection, IntPtr raster,
        ref IntPtr[] rasterBands, ref Int32 nrBands);

    #region rasbandinfo
    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasbandinfo_create(ref IntPtr rasBandInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_rasbandinfo_free(IntPtr rasBandInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasbandinfo_get_band_size(IntPtr rasBandInfo,
        ref Int32 width, ref Int32 height);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasbandinfo_get_extent(IntPtr rasBandInfo,
        IntPtr extent);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasbandinfo_get_pixel_type(IntPtr rasBandInfo,
        ref Int32 pixelType);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasterband_get_info_by_id(IntPtr connection,
        Int32 rasterColId, Int32 bandId, IntPtr rasBandInfo);

    #endregion

    #region rasconstraint
    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasconstraint_create(ref IntPtr rasConstraint);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_rasconstraint_free(IntPtr rasConstraint);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasconstraint_reset(IntPtr rasConstraint);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasconstraint_set_bands(IntPtr rasConstraint, int nrBands,
        int[] bandList);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasconstraint_set_envelope(IntPtr rasConstraint,
        int minx, int miny, int maxx, int maxy);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rasconstraint_set_level(IntPtr rasConstraint, int level);
    #endregion

    #region rastileinfo
    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rastileinfo_create(ref IntPtr rasTileInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rastileinfo_free(IntPtr rasTileInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_rastileinfo_get_pixel_data(IntPtr rasTileInfo,
        ref IntPtr data, ref int length);

    #endregion

    #endregion

    #region shape

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_create(IntPtr Coordref,
        out IntPtr Shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_shape_free(IntPtr Shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_shape_free_array(int nShape, IntPtr[] shapeList);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_generate_point(Int32 num_pts,
        Se_Point[] arrSePoint,
        double[] z,
        double[] measure,
        IntPtr Shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_generate_line(Int32 num_pts,
      Int32 num_parts, Int32[] part_offsets, Se_Point[] point_array,
      double[] z, double[] measure, IntPtr tgt_shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_generate_polygon(Int32 num_pts,
        Int32 num_parts,
        Int32[] part_offsets,
        Se_Point[] arrSePoint,
        double[] z,
        double[] measure,
        IntPtr Shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_generate_rectangle(
        [In] IntPtr pEnvelope,
        IntPtr Shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_as_simple_line(IntPtr srcShape, IntPtr tgtShape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_get_points(IntPtr Shape,
        Int32 part_num,
        Int16 rotEnum,
        Int32[] subpart_offsets,
        [Out] Se_Point[] arrSePoint,
        double[] z,
        double[] measure);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_get_num_parts(IntPtr Shape,
        out int arrNumParts,
        out int arrNumSubParts);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_get_num_subparts(IntPtr Shape,
        int part_num,
        out int arrNumSubParts);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_get_num_points(IntPtr Shape,
        int part_num,
        int subpart_num,
        out int arrNumPts);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_get_extent(IntPtr shape,
      Int32 part_num, ref Se_Envelope envelope);

    #region shape relation

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_crossing(IntPtr primaryShape,
      IntPtr secondaryShape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_disjoint(IntPtr primaryShape,
      IntPtr secondaryShape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_within(IntPtr primaryShape,
      IntPtr secondaryShape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_equal(IntPtr primaryShape,
      IntPtr secondaryShape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_overlapping(IntPtr primaryShape,
      IntPtr secondaryShape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_touching(IntPtr primaryShape,
      IntPtr secondaryShape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_build(IntPtr srcShape, int mode,
      bool dissolve, int matchMask, IntPtr tgtShape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_shape_intersect(IntPtr primaryShape,
      IntPtr secondaryShape, ref int numShape, ref IntPtr[] intersections);

    #endregion shape relation

    #region shape properties

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_point(IntPtr shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_line(IntPtr shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_simple_line(IntPtr shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_multipart(IntPtr shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_polygon(IntPtr shape);

    #endregion

    #region shape Elevation and Measure

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_3D(IntPtr shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern bool SE_shape_is_measured(IntPtr shape);

    #endregion

    #endregion

    #region stream

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_create(IntPtr Connection,
        ref IntPtr Stream);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_free(IntPtr stream);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_bind_output_column(IntPtr Stream,
        Int16 column,
        IntPtr data,
        IntPtr indicator);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_delete_row(IntPtr Stream,
        string table,
        Int32 sde_row_id);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_close(IntPtr Stream,
        Int32 reset);

    //-------------------------------------------------------------------------
    /// <summary>
    /// Purpose: Initiates a layer query.<br/>
    /// Notes:	 <br/>
    /// History: ML 03.09.2003 initial coding
    /// </summary>
    /// <returns>LONG</returns>
    /// <param name="Stream">SE_STREAM stream</param>
    /// <param name="num_columns">SHORT num_columns</param>
    /// <param name="columns">const CHAR **columns</param>
    /// <param name="sqlConstruct">const SE_SQL_CONSTRUCT *construct</param>/>
    //-------------------------------------------------------------------------
    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_query(IntPtr Stream, Int16 num_columns,
        [In, Out] String[] columns, ref Se_Sql_Construct sqlConstruct);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_query_with_info(IntPtr stream, IntPtr queryInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_set_raster_constraint(IntPtr stream,
        IntPtr rasterConstraint);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_query_raster_tile(IntPtr stream, IntPtr rasterConstraint);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_execute(IntPtr Stream);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_fetch(IntPtr Stream);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_fetch_row(IntPtr Stream,
        string table
        , Int32 sde_row_id
        , Int16 num_columns
        , [In, Out] String[] columns);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_get_string(IntPtr Stream,
        Int16 column,
        IntPtr string_val);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_get_nstring(IntPtr Stream,
        Int16 column,
        IntPtr wchar);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_get_double(IntPtr Stream,
        Int16 column,
        out double double_val);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_get_uuid(IntPtr Stream,
        Int16 column,
        IntPtr guid_val);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_get_date(IntPtr Stream,
        Int16 column,
        out tm date_val);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_get_integer(IntPtr Stream,
        Int16 column,
        out int int_val);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_get_smallint(IntPtr Stream,
        Int16 column,
        out Int16 short_val);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_get_raster(IntPtr stream,
        Int16 column, IntPtr rasterAttr);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_get_raster_tile(IntPtr stream, IntPtr rasterTile);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_get_shape(IntPtr Stream,
        Int16 column,
        [In, Out] IntPtr Shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_set_shape(IntPtr Stream,
        Int16 column,
        IntPtr Shape);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_set_integer(IntPtr Stream,
        Int16 column,
        Int32 int_val);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_set_spatial_constraints(IntPtr Stream,
        Int16 search_order,
        bool calc_masks,
        Int16 num_filters,
        [In] Se_Filter[] filters);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_insert_table(IntPtr Stream,
        char[] table,
        Int16 num_columns,
        [In, Out] String[] columns);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_last_inserted_row_id(IntPtr Stream,
        Int32[] arrLastRowId);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_delete_by_id_list(IntPtr stream,
        string table, Int32[] idList, int idCount);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_get_ext_error(
        IntPtr Stream,
        ref Se_Error Error);

    #region version
    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_set_state(IntPtr stream, Int32 sourceId,
        Int32 differencesId, Int32 differenceType);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stream_copy_state_rows(IntPtr stream, string table,
        Int32[] rowIdList, Int32 rowIdCount);
    #endregion

    #endregion

    #region sql

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_sql_construct_alloc(
        Int32 num_tables,
        out IntPtr ppSe_sql_construct);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_sql_construct_free(IntPtr pSe_sql_construct);
    #endregion

    #region state
    #region stateinfo

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stateinfo_create(ref IntPtr stateInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_stateinfo_free(IntPtr stateInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stateinfo_get_id(IntPtr stateInfo, ref Int32 stateId);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_stateinfo_get_closing_time(IntPtr stateInfo, ref tm time);

    #endregion

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_state_close(IntPtr connection, Int32 stateId);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_state_open(IntPtr connection, Int32 stateId);
    #endregion

    #region table

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern unsafe Int32 SE_table_list_(IntPtr connection,
      int permissions,
      out int num_tables,
      byte*** tables);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_table_list(IntPtr connection,
      int permissions,
      out int num_tables,
      out IntPtr tables);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_table_free_list
      (int num_tables,
      IntPtr tables);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_table_create(IntPtr connection,
        string table,
        Int16 num_columns,
        Se_Column_Def[] column_defs,
        string config_keyword);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_table_create_view(IntPtr connection,
      string view,
      Int16 num_view_columns,
      Int16 num_table_columns,
      IntPtr[] view_columns,
      IntPtr[] table_columns,
      ref Se_Sql_Construct sqlc);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_table_delete(IntPtr connection,
        string table);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_table_get_column_list(IntPtr conn, string _name, out IntPtr columns,
                                                          out short nColumns);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_table_free_column_list(short nColumns, IntPtr columns);

    #region columninfo

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern int SE_columninfo_get_decimal_digits(IntPtr se_columnInfo, out int decimal_digits);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern int SE_columninfo_get_description(IntPtr info, [In, Out] char[] description);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern int SE_columninfo_get_name(IntPtr info, [In, Out] char[] name);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern int SE_columninfo_get_type(IntPtr info, out int type);

    #endregion

    #endregion

    #region versioninfo
    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_versioninfo_create(ref IntPtr versionInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern void SE_versioninfo_free(IntPtr versionInfo);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_versioninfo_get_state_id(IntPtr versionInfo, ref Int32 stateId);

    [DllImport(sdeDll, SetLastError = true, ThrowOnUnmappableChar = true)]
    internal static extern Int32 SE_versioninfo_set_state_id(IntPtr versionInfo, Int32 stateId);

    #endregion

  }

  #region structs

  [StructLayout(LayoutKind.Sequential)]
  public struct Se_Error
  {
    /// <summary>
    /// C-Def: LONG sde_error
    /// </summary>
    public Int32 sde_error;
    /// <summary>
    /// C-Def: LONG ext_error
    /// </summary>
    public Int32 ext_error;
    /// <summary>
    /// C-Def: CHAR err_msg1[SE_MAX_MESSAGE_LENGTH] -> [ MarshalAs( UnmanagedType.ByValArray, SizeConst=SdeType.SE_MAX_MESSAGE_LENGTH)] 
    /// </summary>
    //    [MarshalAs(UnmanagedType.ByValArray, SizeConst = SdeType.SE_MAX_MESSAGE_LENGTH)]
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdeType.SE_MAX_MESSAGE_LENGTH + 1)]
    public string err_msg1;
    /// <summary>
    /// C-Def: CHAR err_msg2[SE_MAX_SQL_MESSAGE_LENGTH] -> [ MarshalAs( UnmanagedType.ByValArray, SizeConst=SdeType.SE_MAX_SQL_MESSAGE_LENGTH)] 
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdeType.SE_MAX_SQL_MESSAGE_LENGTH + 1)]
    public string err_msg2;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct Se_Sql_Construct : IDisposable
  {
    /// <summary>
    /// C-Def: LONG num_tables
    /// </summary>
    private Int32 num_tables;
    ///// <summary>
    ///// C-Def: CHAR **tables
    ///// </summary>
    private IntPtr tables;
    /// <summary>
    /// C-Def: CHAR *where
    /// </summary>
    private string where;

    public Se_Sql_Construct(string[] tables, string where)
    {
      num_tables = tables.Length;
      IntPtr values = Api.GetIntPtr(tables);
      this.tables = values;
      this.where = where;
    }

    public void Dispose()
    {
      for (int i = 0; i < num_tables; i++)
      {
        unsafe
        {
          Marshal.FreeHGlobal(new IntPtr(((byte**)tables)[i]));
        }
      }

      Marshal.FreeHGlobal(tables);
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct Se_Point
  {
    public double X;
    public double Y;

    public Se_Point(double x, double y)
    {
      X = x;
      Y = y;
    }
  }

  public struct Se_Envelope
  {
    /// <summary>
    /// C-Def: LFLOAT minx
    /// </summary>
    public double minx;
    /// <summary>
    /// C-Def: LFLOAT miny
    /// </summary>
    public double miny;
    /// <summary>
    /// C-Def: LFLOAT maxx
    /// </summary>
    public double maxx;
    /// <summary>
    /// C-Def: LFLOAT maxy
    /// </summary>
    public double maxy;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct Se_Column_Def
  {
    /// <summary>
    /// C-Def: CHAR  column_name[SE_MAX_COLUMN_LEN] -> [ MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string column_name;

    /// <summary>
    /// C-Def: LONG  sde_type
    /// </summary>
    public Int32 sde_type;
    /// <summary>
    /// C-Def: LONG  size: the size of the column values
    /// </summary>
    public Int32 size;
    /// <summary>
    /// C-Def: SHORT decimal_digits: number of digits after decimal
    /// </summary>
    public Int16 decimal_digits;
    private Int32 nulls_allowed;
    /// <summary>
    /// C-Def: SHORT row_id_type: column's use as table's row id
    /// </summary>
    public Int16 row_id_type;

    public bool Nulls_Allowed
    { // Methode, wird nicht gemarshalled
      get { return Convert.ToBoolean(nulls_allowed); }
      set { nulls_allowed = Convert.ToInt32(value); }
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct Se_Filter
  {
    public Se_Filter(string tableName, string shapeCol, Filter f, int m)
    {
      table = new char[226];
      column = new char[32];
      {
        int i = 0;
        foreach (char c in tableName)
        {
          table[i] = c;
          i++;
        }
      }
      {
        int i = 0;
        foreach (char c in shapeCol)
        {
          column[i] = c;
          i++;
        }
      }
      cbm_source = null;
      cnm_object_code = null;
      filter = f;
      filter_type = SdeType.SE_SHAPE_FILTER;
      method = m;
      truth = true;
    }
    /// <summary>
    /// C-Def: CHAR table[SE_QUALIFIED_TABLE_NAME]: the spatial table name -> [ MarshalAs(UnmanagedType.ByValArray, SizeConst=226)]
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 226)]
    public char[] table;
    /// <summary>
    /// C-Def: CHAR column[SE_MAX_COLUMN_LEN]: the spatial column name -> [ MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
    public char[] column;
    /// <summary>
    /// C-Def: LONG filter_type: the type of spatial filter
    /// </summary>
    public Int32 filter_type;
    /// <summary>
    /// C-Def: union filter
    /// </summary>
    public Filter filter;
    /// <summary>
    /// C-Def: LONG method: the search method to satisfy
    /// </summary>
    public Int32 method;
    private bool truth;
    /// <summary>
    /// C-Def: char *cbm_source: set ONLY if the method is SM_CBM
    /// </summary>
    public string cbm_source;
    /// <summary>
    /// C-Def: UCHAR *cbm_object_code: internal system use only
    /// </summary>
    public string cnm_object_code;

    /// <summary>
    /// C-Def: BOOL truth
    /// </summary>
    /// <value name="Truth">TRUE to pass the test, FALSE if it must NOT pass</value>
    public bool Truth
    {
      get { return Convert.ToBoolean(truth); }
      set { truth = value; }
    }
    /*public string Cbm_source
    {
        get{ return Marshal.PtrToStringAnsi(cbm_source); }
    }
    public string Cnm_object_code
    {
        get{ return Marshal.PtrToStringAnsi(cnm_object_code); }
    }*/
  }

  [StructLayout(LayoutKind.Sequential, Size = 232)]//Size=232
  public struct Filter
  {
    /// <summary>
    /// C-Def: SE_SHAPE shape: a shape object
    /// </summary>
    //[FieldOffset(0)]
    public IntPtr shape;
    //[FieldOffset(0)]
    //public IntPtr id;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct Id
  {
    /// <summary>
    /// C-Def: SE_SHAPE shape: a shape object
    /// </summary>
    public Int32 id;
    /// <summary>
    /// C-Def: CHAR table[SE_QUALIFIED_TABLE_NAME]: The shape's spatial table -> [ MarshalAs(UnmanagedType.ByValArray, SizeConst=226)]
    /// </summary>
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 226)]
    public char[] table;
  }


  [StructLayout(LayoutKind.Sequential)]
  public struct Se_Instance_User
  {
    /// <summary>
    /// C-Def: LONG 
    /// </summary>
    public Int32 svrpid;
    /// <summary>
    /// C-Def: LONG 
    /// </summary>
    public Int32 cstime;

    /// <summary>
    /// C-Def: 	BOOL  nulls_allowed
    /// </summary>
    /// <value name"Nulls_Allowed">True when nulls allowed.</value>
    public bool Xdr_needed
    {
      get { return Convert.ToBoolean(_xdr_needed); }
      set { _xdr_needed = Convert.ToInt32(value); }
    }
    private Int32 _xdr_needed;
    /// <summary>
    /// C-Def: CHAR err_msg1[SE_MAX_OWNER_LEN + 1]
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdeType.SE_MAX_OWNER_LEN + 1)]
    public string sysname;
    /// <summary>
    /// C-Def: CHAR err_msg1[SE_MAX_OWNER_LEN + 1]
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdeType.SE_MAX_OWNER_LEN + 1)]
    public string nodename;
    /// <summary>
    /// C-Def: CHAR err_msg1[SE_MAX_OWNER_LEN + 1]
    /// </summary>
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SdeType.SE_MAX_OWNER_LEN + 1)]
    public string username;

  }

  struct tm
  {
    public tm(DateTime tmVal)
    {
      tm_sec = tmVal.Second;
      tm_min = tmVal.Minute;
      tm_hour = tmVal.Hour;
      tm_mday = tmVal.Day;
      tm_mon = tmVal.Month - 1;
      tm_year = tmVal.Year - 1900;
      tm_wday = (int)tmVal.DayOfWeek;
      tm_yday = tmVal.DayOfYear;
      TimeZone tz = TimeZone.CurrentTimeZone;
      tm_isdst = tz.IsDaylightSavingTime(tmVal) ? 1 : 0;
    }
    public int tm_sec;       // Seconds after the minute
    public int tm_min;       // Minutes after the hour 
    public int tm_hour;      // Hours since midnight
    public int tm_mday;      // The day of the month
    public int tm_mon;       // The month (January = 0)
    public int tm_year;      // The year (00 = 1900)
    public int tm_wday;      // The day of the week (Sunday = 0)
    public int tm_yday;      // The day of the year (Jan. 1 = 1)
    public int tm_isdst;     // Flag to indicate if DST is in effect
    public override string ToString()
    {
      const string wDays = "SunMonTueWedThuFriSat";
      const string months = "JanFebMarAprMayJunJulAugSepOctNovDec";
      return (String.Format("{0} {1} {2,2:00} " +
                      "{3,2:00}:{4,2:00}:{5,2:00} {6}\n",
                       wDays.Substring(3 * tm_wday, 3),
                       months.Substring(3 * tm_mon, 3),
                       tm_mday, tm_hour, tm_min,
                       tm_sec, tm_year + 1900));
    }

    public bool IsNull()
    {
      bool isNull = tm_year == 0 && tm_yday == 0 && tm_mon == 0 && tm_mday == 0 &&
        tm_hour == 0 && tm_min == 0 && tm_sec == 0;
      return isNull;
    }
    public DateTime GetDate()
    {
      DateTime dt = new DateTime(tm_year + 1900, tm_mon + 1, tm_mday, tm_hour, tm_min, tm_sec);
      return dt;
    }
  }

  #endregion
}
