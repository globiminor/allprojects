using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace ArcSde
{
  //-------------------------------------------------------------------------
  /// <summary>
  /// Purpose: Wrapperklasse für den Zugriff auf die C-API von ArcSDE 8.3.
  ///		Die statischen Methoden rufen die Representationen der "native" in
  ///		.net auf und werfen nötigenfalls eine ArcSdeException.<br/>
  /// Notes:	Die in ArcSde generierten Fehlerangaben werden als "Return
  ///		Code" in den managed Code übermittelt<br/>
  ///		Weitergehende Dokumentationen zu allen Methoden von API:
  ///		 <A href="http://arcsdeonline.esri.com/index.htm">ArcSDE Developer Help, Developer Interface, C API</A><br/>
  /// History: ML 03.09.2003 initial coding
  /// </summary>
  //-------------------------------------------------------------------------
  public class Api
  {
    #region nested classes
    private static class PointerToStruct
    {
      public unsafe static object Convert(IntPtr lst, object x)
      {
        IntPtr pos = lst;
        Type t = x.GetType();

        foreach (FieldInfo info in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
          if (info.FieldType == typeof(Int32))
          {
            int* p = (int*)pos.ToPointer();
            info.SetValue(x, *p);
            pos = new IntPtr(pos.ToInt64() + sizeof(Int32));
          }
          else if (info.FieldType == typeof(string))
          {
            object[] attrs = info.GetCustomAttributes(typeof(MarshalAsAttribute), false);
            MarshalAsAttribute mattr = (MarshalAsAttribute)attrs[0];
            int nChar = mattr.SizeConst;

            string s = "";
            for (int i = 0; i < nChar; i++)
            {
              s += (char)*((byte*)pos.ToPointer());
              pos = new IntPtr(pos.ToInt64() + sizeof(byte));
            }
            info.SetValue(x, s);
          }
          else
          {
            throw new NotImplementedException();
          }
        }
        return x;
      }
    }
    #endregion

    public static string String(char[] chars)
    {
      StringBuilder s = new StringBuilder(chars.Length);
      int i = 0;
      while (chars[i] != 0)
      {
        s.Append(chars[i]);
        i++;
      }
      return s.ToString();
    }
    public static IntPtr GetIntPtr(string[] values)
    {
      int nValues = values.Length;
      int bytePtrSize = Marshal.SizeOf(typeof(byte*));
      IntPtr ptr = Marshal.AllocHGlobal(bytePtrSize * nValues);
      unsafe
      {
        byte** vPtr = (byte**)ptr.ToPointer();
        for (int iValue = 0; iValue < nValues; iValue++)
        {
          vPtr[iValue] = (byte*)Marshal.StringToHGlobalAnsi(values[iValue]);
        }
      }
      return ptr;
    }

    #region instance

    public static void SE_instance_get_users(string server, string instance,
      ref IntPtr userList, ref int userCount)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_instance_get_users(server, instance, ref userList, ref userCount));
    }

    public static Se_Instance_User[] SE_instance_get_users(string server, string instance)
    {
      IntPtr lst = IntPtr.Zero;
      int userCount = 0;
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_instance_get_users(server, instance, ref lst, ref userCount));

      Se_Instance_User[] userList = new Se_Instance_User[userCount];
      int size = Marshal.SizeOf(typeof(Se_Instance_User));
      for (int i = 0; i < userCount; i++)
      {
        userList[i] = (Se_Instance_User)PointerToStruct.Convert(
          new IntPtr(lst.ToInt64() + i * size), new Se_Instance_User());
      }

      CApi.SE_instance_free_users(lst, userCount);

      return userList;
    }

    public static bool SE_instance_control(string server, string instance, string passwd,
      int instanceControl, int pid, out string errorMsg)
    {
      int rc = CApi.SE_instance_control(server, instance, passwd, instanceControl, pid);
      return ErrorHandling.IsNoError(rc, out errorMsg);
    }

    #endregion

    #region connection

    #region state

    internal static void SE_state_get_info(IntPtr connection, Int32 stateID, IntPtr stateInfo)
    {
      ErrorHandling.CheckRC(connection, IntPtr.Zero,
        CApi.SE_state_get_info(connection, stateID, stateInfo));
    }

    internal static void SE_state_create(IntPtr connection,
      IntPtr stateInfo, Int32 parentStateId, IntPtr resultStateInfo)
    {
      ErrorHandling.CheckRC(connection, IntPtr.Zero,
        CApi.SE_state_create(connection, stateInfo, parentStateId, resultStateInfo));
    }

    public static void SE_state_new_edit(IntPtr connection,
      IntPtr stateInfo, Int32 parentStateId, IntPtr resultStateInfo)
    {
      ErrorHandling.CheckRC(connection, IntPtr.Zero,
        CApi.SE_state_new_edit(connection, stateInfo, parentStateId, resultStateInfo));
    }

    #endregion

    #region version
    internal static void SE_version_get_info(IntPtr connection, string versionName, IntPtr versionInfo)
    {
      ErrorHandling.CheckRC(connection, IntPtr.Zero,
        CApi.SE_version_get_info(connection, versionName, versionInfo));
    }

    internal static void SE_version_change_state(IntPtr connection, IntPtr versionInfo, Int32 newStateId)
    {
      ErrorHandling.CheckRC(connection, IntPtr.Zero,
        CApi.SE_version_change_state(connection, versionInfo, newStateId));
    }

    #endregion

    public static void SE_connection_create(string server,
      string instance,
      string database,
      string username,
      string password,
      ref Se_Error error,
      ref IntPtr conn)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_connection_create(server, instance, database, username,
        password, ref error, ref conn));
    }

    internal static void SE_connection_start_transaction(IntPtr conn)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_connection_start_transaction(conn));
    }

    internal static void SE_connection_rollback_transaction(IntPtr conn)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_connection_rollback_transaction(conn));
    }

    internal static void SE_connection_commit_transaction(IntPtr conn)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_connection_commit_transaction(conn));
    }

    public static void SE_connection_get_ext_error(IntPtr Connection,
      ref Se_Error Error)
    {
      ErrorHandling.CheckRC(Connection, IntPtr.Zero,
        CApi.SE_connection_get_ext_error(Connection, ref Error));
    }

    public static void SE_connection_free(IntPtr conn)
    {
      CApi.SE_connection_free(conn);
    }

    #endregion

    #region coordref

    public static void SE_coordref_create(ref IntPtr coordref)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_coordref_create(out coordref));
    }

    public static void SE_coordref_set_xy(IntPtr coordref,
      Single falsex,
      Single falsey,
      Single xyunits)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_coordref_set_xy(coordref, falsex, falsey, xyunits));
    }

    public static void SE_coordref_free(IntPtr coordref)
    {
      CApi.SE_coordref_free(coordref);
    }
    #endregion

    #region layerinfo

    public static void SE_layerinfo_create(IntPtr coordref,
      out IntPtr layerinfo)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_layerinfo_create(coordref, out layerinfo));
    }

    public static void SE_layerinfo_set_spatial_column(IntPtr layerinfo,
      string table,
      string column)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_layerinfo_set_spatial_column(layerinfo, table, column));
    }

    public static void SE_layerinfo_get_spatial_column(IntPtr Layerinfo,
      char[] table,
      char[] column)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_layerinfo_get_spatial_column(Layerinfo, table, column));
    }

    public static void SE_layerinfo_get_coordref(IntPtr Layerinfo,
      IntPtr Coordref)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_layerinfo_get_coordref(Layerinfo, Coordref));
    }

    public static void SE_layerinfo_set_shape_types(IntPtr layerinfo,
      Int32 shape_types)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_layerinfo_set_shape_types(layerinfo, shape_types));
    }

    public static void SE_layerinfo_get_shape_types(
      IntPtr layerinfo,
      out Int32 shape_types)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_layerinfo_get_shape_types(layerinfo, out shape_types));
    }

    public static void SE_layerinfo_set_grid_sizes(IntPtr layerinfo,
      double grid_size,
      double grid_size2,
      double grid_size3)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_layerinfo_set_grid_sizes(layerinfo, grid_size, grid_size2,
        grid_size3));
    }

    public static void SE_layerinfo_get_grid_sizes(IntPtr layerinfo,
      out double grid_size,
      out double grid_size2,
      out double grid_size3)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_layerinfo_get_grid_sizes(layerinfo, out grid_size,
        out grid_size2, out grid_size3));
    }

    public static void SE_layerinfo_set_creation_keyword(IntPtr layerinfo,
      string config_keyword)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_layerinfo_set_creation_keyword(layerinfo, config_keyword));
    }

    public static void SE_layerinfo_free(IntPtr Layerinfo)
    {
      CApi.SE_layerinfo_free(Layerinfo);
    }
    #endregion

    #region layer

    public static void SE_layer_get_info_by_id(SeConnection connection,
      Int32 layer_id,
      IntPtr Layer)
    {
      ErrorHandling.CheckRC(connection.Conn, IntPtr.Zero,
        CApi.SE_layer_get_info_by_id(connection.Conn, layer_id, Layer));
    }

    public static void SE_layer_create(IntPtr Connection,
      IntPtr Layerinfo,
      Single initialf,
      Single avg_points)
    {
      ErrorHandling.CheckRC(Connection, IntPtr.Zero,
        CApi.SE_layer_create(Connection, Layerinfo, initialf, avg_points));
    }
    #endregion

    #region error

    public static void SE_error_get_string(Int32 error_code,
      char[] error_string)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_error_get_string(error_code, error_string));
    }
    #endregion

    #region query
    public static void SE_queryinfo_create(ref IntPtr queryInfo)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_queryinfo_create(ref queryInfo));
    }
    public static void SE_queryinfo_free(IntPtr queryInfo)
    {
      CApi.SE_queryinfo_free(queryInfo);
    }

    public static void SE_queryinfo_set_tables(IntPtr queryInfo, string[] tableList, string[] aliasList)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_queryinfo_set_tables(queryInfo, tableList.Length, tableList, aliasList));
    }

    public static void SE_queryinfo_set_columns(IntPtr queryInfo, string[] columnList)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_queryinfo_set_columns(queryInfo, columnList.Length, columnList));
    }

    #endregion

    #region raster
    #region rasterinfo
    public static void SE_rasterinfo_create(ref IntPtr raster)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasterinfo_create(ref raster));
    }

    public static void SE_rasterinfo_free(IntPtr raster)
    {
      CApi.SE_rasterinfo_free(raster);
    }
    #endregion

    #region rasterattr
    public static void SE_rasterattr_create(ref IntPtr rasAttr, bool inputMode)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasterattr_create(ref rasAttr, inputMode));
    }

    public static void SE_rasterattr_free(IntPtr rasAttr)
    {
      CApi.SE_rasterattr_free(rasAttr);
    }

    public static void SE_rasterattr_get_extent_by_level(IntPtr rasterAttr,
      ref Se_Envelope extent, ref double coordOffsetX, ref double coordOffsetY, int level)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasterattr_get_extent_by_level(rasterAttr,
        ref extent, ref coordOffsetX, ref coordOffsetY, level));
    }
    public static void SE_rasterattr_get_image_size_by_level(IntPtr rasterAttr,
      ref int pixelWidth, ref int pixelHeight, ref int pixelOffetX, ref int pixelOffsetY,
      ref int nrBands, int level)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasterattr_get_image_size_by_level(rasterAttr,
      ref pixelWidth, ref pixelHeight, ref pixelOffetX, ref pixelOffsetY,
      ref nrBands, level));
    }
    public static void SE_rasterattr_get_tile_size(IntPtr rasterAttr, ref int tileWidth, ref int tileHeight)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasterattr_get_tile_size(rasterAttr, ref tileWidth, ref tileHeight));
    }
    #endregion

    #region rascolinfo
    public static void SE_rascolinfo_create(ref IntPtr rasColInfo)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rascolinfo_create(ref rasColInfo));
    }

    public static void SE_rascolinfo_free(IntPtr rasColInfo)
    {
      CApi.SE_rascolinfo_free(rasColInfo);
    }

    public static void SE_rascolinfo_get_id(IntPtr rasColInfo, ref int rasterColumnId)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rascolinfo_get_id(rasColInfo, ref rasterColumnId));
    }

    public static void SE_rascolinfo_get_raster_column(IntPtr rasColInfo, out string table, out string col)
    {
      char[] t = new char[SdeType.SE_QUALIFIED_TABLE_NAME];
      char[] c = new char[SdeType.SE_MAX_COLUMN_LEN];
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rascolinfo_get_raster_column(rasColInfo, t, c));
      table = new string(t);
      col = new string(c);
    }

    public static void SE_rastercolumn_get_info_list(IntPtr connection,
      out IntPtr[] rascol_list)
    {
      unsafe
      {
        ErrorHandling.CheckRC(connection, IntPtr.Zero,
        CApi.SE_rastercolumn_get_info_list(connection, out IntPtr* pos, out int count));

        rascol_list = new IntPtr[count];
        for (int i = 0; i < count; i++)
        {
          rascol_list[i] = pos[i];
        }
      }
    }

    public static void SE_rastercolumn_get_info_by_name(IntPtr connection,
      string table, string column, IntPtr rasColInfo)
    {
      ErrorHandling.CheckRC(connection, IntPtr.Zero,
        CApi.SE_rastercolumn_get_info_by_name(connection, table, column, rasColInfo));
    }
    #endregion

    #region raster
    public static void SE_raster_get_info_by_id(IntPtr connection,
      int columnId, int rasterId, IntPtr rasterInfo)
    {
      ErrorHandling.CheckRC(connection, IntPtr.Zero,
        CApi.SE_raster_get_info_by_id(connection, columnId, rasterId, rasterInfo));
    }

    public static void SE_raster_get_bands(IntPtr connection, IntPtr raster, ref IntPtr[] rasterBands)
    {
      int nrBands = 0;
      ErrorHandling.CheckRC(connection, IntPtr.Zero,
        CApi.SE_raster_get_bands(connection, raster, ref rasterBands, ref nrBands));
    }
    #endregion

    #region rasbandinfo
    public static void SE_rasbandinfo_create(ref IntPtr rasBandInfo)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasbandinfo_create(ref rasBandInfo));
    }
    public static void SE_rasbandinfo_free(IntPtr rasBandInfo)
    {
      CApi.SE_rasbandinfo_free(rasBandInfo);
    }
    public static void SE_rasbandinfo_get_band_size(IntPtr rasBandInfo,
      ref Int32 width, ref Int32 height)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasbandinfo_get_band_size(rasBandInfo, ref width, ref height));
    }
    public static void SE_rasbandinfo_get_extent(IntPtr rasBandInfo, ref Se_Envelope extent)
    {
      IntPtr ptrExtent = IntPtr.Zero;
      try
      {
        ptrExtent = Marshal.AllocHGlobal(Marshal.SizeOf(extent));
        Marshal.StructureToPtr(extent, ptrExtent, false);

        //				ptrExtent = Marshal.GetIDispatchForObject(extent);
        ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
          CApi.SE_rasbandinfo_get_extent(rasBandInfo, ptrExtent));

        extent = (Se_Envelope)Marshal.PtrToStructure(ptrExtent, typeof(Se_Envelope));
      }
      finally
      {
        Marshal.FreeHGlobal(ptrExtent);
      }
    }
    public static void SE_rasbandinfo_get_pixel_type(IntPtr rasBandInfo, ref Int32 pixelType)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasbandinfo_get_pixel_type(rasBandInfo, ref pixelType));
    }
    public static void SE_rasterband_get_info_by_id(IntPtr connection, Int32 rasterColId,
      Int32 bandId, IntPtr rasBandInfo)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasterband_get_info_by_id(connection, rasterColId, bandId, rasBandInfo));
    }
    #endregion

    #region rasconstraint
    public static void SE_rasconstraint_create(ref IntPtr rasConstraint)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasconstraint_create(ref rasConstraint));
    }
    public static void SE_rasconstraint_free(IntPtr rasConstraint)
    {
      CApi.SE_rasconstraint_free(rasConstraint);
    }
    public static void SE_rasconstraint_reset(IntPtr rasConstraint)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasconstraint_reset(rasConstraint));
    }
    public static void SE_rasconstraint_set_bands(IntPtr rasConstraint,
      int[] bandList)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasconstraint_set_bands(rasConstraint, bandList.Length, bandList));
    }
    public static void SE_rasconstraint_set_envelope(IntPtr rasConstraint,
      int minx, int miny, int maxx, int maxy)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasconstraint_set_envelope(rasConstraint, minx, miny, maxx, maxy));
    }
    public static void SE_rasconstraint_set_level(IntPtr rasConstraint, int level)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rasconstraint_set_level(rasConstraint, level));
    }

    #endregion

    #region rastileinfo
    public static void SE_rastileinfo_create(ref IntPtr rasTileInfo)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_rastileinfo_create(ref rasTileInfo));
    }

    public static void SE_rastileinfo_free(IntPtr rasTileInfo)
    {
      CApi.SE_rastileinfo_free(rasTileInfo);
    }

    public static void SE_rastileinfo_get_pixel_data(IntPtr rasTileInfo, float[] data)
    {
      unsafe
      {
        // TODO: different types of data
        IntPtr ptrData = IntPtr.Zero;
        int length = 0;
        int l;
        float* res;

        ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
          CApi.SE_rastileinfo_get_pixel_data(rasTileInfo, ref ptrData, ref length));
        res = (float*)ptrData.ToPointer();
        l = data.Length;

        for (int i = 0; i < l; i++)
        {
          ((float[])data)[i] = res[i];
        }
      }
    }

    #endregion
    #endregion

    #region shape

    public static void SE_shape_create(IntPtr Coordref,
      ref IntPtr Shape)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_shape_create(Coordref, out Shape));
    }

    public static void SE_shape_free(IntPtr Shape)
    {
      CApi.SE_shape_free(Shape);
    }

    public static void SE_shape_generate_point(Int32 num_pts,
      Se_Point[] arrSePoint,
      double[] z,
      double[] measure,
      IntPtr Shape)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_shape_generate_point(num_pts, arrSePoint, z, measure, Shape));
    }

    public static void SE_shape_generate_polygon(Int32 num_pts,
      Int32 num_parts,
      Int32[] part_offsets,
      Se_Point[] arrSePoint,
      double[] z,
      double[] measure,
      IntPtr Shape)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_shape_generate_polygon(num_pts, num_parts, part_offsets,
        arrSePoint, z, measure, Shape));
    }

    public static void SE_shape_generate_rectangle(IntPtr pEnvelope,
      IntPtr Shape)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_shape_generate_rectangle(pEnvelope, Shape));
    }

    public static void SE_shape_get_points(IntPtr Shape,
      Int32 part_num,
      Int16 rotEnum,
      Int32[] subpart_offsets,
      Se_Point[] arrSePoint,
      double[] z,
      double[] measure)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_shape_get_points(Shape, part_num, rotEnum, subpart_offsets,
        arrSePoint, z, measure));
    }

    public static void SE_shape_get_num_parts(IntPtr Shape,
      out int arrNumParts,
      out int arrNumSubParts)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_shape_get_num_parts(Shape, out arrNumParts, out arrNumSubParts));
    }

    public static void SE_shape_get_num_subparts(IntPtr Shape,
      int part_num,
      out int arrNumSubParts)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_shape_get_num_subparts(Shape, part_num, out arrNumSubParts));
    }

    public static void SE_shape_get_num_points(IntPtr Shape,
      int part_num,
      int subpart_num,
      out int arrNumPts)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_shape_get_num_points(Shape, part_num, subpart_num, out arrNumPts));
    }
    #endregion

    #region stream

    #region handling

    public static void SE_stream_create(IntPtr Connection,
      ref IntPtr Stream)
    {
      ErrorHandling.CheckRC(Connection, IntPtr.Zero,
        CApi.SE_stream_create(Connection, ref Stream));
    }

    public static void SE_stream_execute(IntPtr Stream)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, Stream,
        CApi.SE_stream_execute(Stream));
    }

    public static void SE_stream_close(IntPtr Stream,
      Int32 reset)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, Stream,
        CApi.SE_stream_close(Stream, reset));
    }

    public static void SE_stream_free(IntPtr stream)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, stream,
        CApi.SE_stream_free(stream));
    }
    #endregion

    #region data

    public static void SE_stream_bind_output_column(IntPtr Stream,
      Int16 column,
      IntPtr data,
      out Int16 indicator)
    {
      throw new NotImplementedException();
      //ErrorHandling.checkRC(IntPtr.Zero, Stream,
      //  CApi.SE_stream_bind_output_column(Stream, column, data,
      //    out indicator));
    }

    public static void SE_stream_delete_row(IntPtr Stream,
      string table,
      Int32 sde_row_id)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, Stream,
        CApi.SE_stream_delete_row(Stream, table, sde_row_id));
    }

    public static void SE_stream_query(IntPtr stream,
      String[] columnList,
      Se_Sql_Construct sqlConstruct)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, stream,
        CApi.SE_stream_query(stream, (short)columnList.Length, columnList, ref sqlConstruct));
    }

    public static void SE_stream_query_with_info(IntPtr stream, IntPtr queryInfo)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, stream,
        CApi.SE_stream_query_with_info(stream, queryInfo));
    }

    public static void SE_stream_set_raster_constraint(IntPtr stream, IntPtr rasterConstraint)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, stream,
        CApi.SE_stream_set_raster_constraint(stream, rasterConstraint));
    }

    public static void SE_stream_query_raster_tile(IntPtr stream, IntPtr rasterConstraint)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, stream,
        CApi.SE_stream_query_raster_tile(stream, rasterConstraint));
    }

    public static Int32 SE_stream_fetch(IntPtr stream)
    {
      int rc = CApi.SE_stream_fetch(stream);
      ErrorHandling.CheckRC(IntPtr.Zero, stream, rc);
      return rc;
    }

    public static void SE_stream_fetch_row(IntPtr Stream,
      string table
      , Int32 sde_row_id
      , Int16 num_columns
      , String[] columns)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, Stream,
        CApi.SE_stream_fetch_row(Stream, table, sde_row_id, num_columns,
        columns));
    }

    [Obsolete]
    public static void SE_stream_get_string(IntPtr Stream,
      Int16 column,
      string string_val)
    {
      throw new NotImplementedException();
//      ErrorHandling.checkRC(IntPtr.Zero, Stream,
//        CApi.SE_stream_get_string(Stream, column, string_val));
    }

    [Obsolete]
    public static void SE_stream_get_double(IntPtr Stream,
      Int16 column,
      Double[] double_val)
    {
      throw new NotImplementedException();
      //ErrorHandling.checkRC(IntPtr.Zero, Stream,
      //  CApi.SE_stream_get_double(Stream, column, double_val));
    }

    [Obsolete]
    public static void SE_stream_get_integer(IntPtr Stream,
      Int16 column,
      Int32[] int_val)
    {
      throw new NotImplementedException();
      //ErrorHandling.checkRC(IntPtr.Zero, Stream,
      //  CApi.SE_stream_get_integer(Stream, column, int_val));
    }

    public static void SE_stream_get_raster(IntPtr stream,
      Int16 column, IntPtr rasterAttr)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, stream,
        CApi.SE_stream_get_raster(stream, column, rasterAttr));
    }

    public static int SE_stream_get_raster_tile(IntPtr stream, IntPtr rasterTile)
    {
      int res = CApi.SE_stream_get_raster_tile(stream, rasterTile);
      ErrorHandling.CheckRC(IntPtr.Zero, stream, res);
      return res;
    }

    public static void SE_stream_get_shape(IntPtr Stream,
      Int16 column,
      IntPtr Shape)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, Stream,
        CApi.SE_stream_get_shape(Stream, column, Shape));
    }

    public static void SE_stream_set_shape(IntPtr Stream,
      Int16 column,
      IntPtr Shape)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, Stream,
        CApi.SE_stream_set_shape(Stream, column, Shape));
    }

    public static void SE_stream_set_integer(IntPtr Stream,
      Int16 column,
      Int32 int_val)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, Stream,
        CApi.SE_stream_set_integer(Stream, column, int_val));
    }

    public static void SE_stream_set_spatial_constraints(IntPtr Stream,
      Int16 search_order,
      bool calc_masks,
      Int16 num_filters,
      Se_Filter[] filters)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, Stream,
        CApi.SE_stream_set_spatial_constraints(Stream, search_order,
        calc_masks, num_filters, filters));
    }

    public static void SE_stream_insert_table(IntPtr Stream,
      char[] table,
      Int16 num_columns,
      String[] columns)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, Stream,
        CApi.SE_stream_insert_table(Stream, table, num_columns, columns));
    }

    public static void SE_stream_last_inserted_row_id(IntPtr Stream,
      Int32[] arrLastRowId)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, Stream,
        CApi.SE_stream_last_inserted_row_id(Stream, arrLastRowId));
    }

    internal static void SE_stream_delete_by_id_list(IntPtr stream,
      string table, Int32[] idList)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, stream,
        CApi.SE_stream_delete_by_id_list(stream, table, idList, idList.Length));
    }
    #endregion

    #region error

    public static void SE_stream_get_ext_error(IntPtr Stream,
      ref Se_Error Error)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, Stream,
        CApi.SE_stream_get_ext_error(Stream, ref Error));
    }
    #endregion

    #region version

    internal static void SE_stream_set_state(IntPtr stream,
      Int32 sourceId, Int32 differencesId, Int32 differenceType)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, stream,
        CApi.SE_stream_set_state(stream, sourceId, differencesId, differenceType));
    }

    internal static void SE_stream_copy_state_rows(IntPtr stream,
      string table, Int32[] rowIdList)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, stream,
        CApi.SE_stream_copy_state_rows(stream, table, rowIdList, rowIdList.Length));
    }

    #endregion

    #endregion

    #region sql
    //-------------------------------------------------------------------------
    /// <summary>
    /// Purpose: Allocates an SE_SQL_CONSTRUCT structure.<br/>
    /// Notes:	 <br/>
    /// History: ML 03.09.2003 initial coding
    /// </summary>
    /// <returns>void</returns>
    /// <param name="num_tables">LONG num_tables</param>
    /// <param name="ppSe_sql_construct">SE_SQL_CONSTRUCT **constructor</param>
    //-------------------------------------------------------------------------
    public static void SE_sql_construct_alloc(Int32 num_tables,
      out IntPtr ppSe_sql_construct)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_sql_construct_alloc(num_tables, out ppSe_sql_construct));
    }

    //-------------------------------------------------------------------------
    /// <summary>
    /// Purpose: Frees an SE_SQL_CONSTRUCT structure.<br/>
    /// Notes:	 <br/>
    /// History: ML 03.09.2003 initial coding
    /// </summary>
    /// <returns>void</returns>
    /// <param name="pSe_sql_construct">SE_SQL_CONSTRUCT *constructor</param>
    //-------------------------------------------------------------------------
    public static void SE_sql_construct_free(IntPtr pSe_sql_construct)
    {
      CApi.SE_sql_construct_free(pSe_sql_construct);
    }
    #endregion

    #region state

    #region stateinfo

    internal static void SE_stateinfo_create(ref IntPtr stateInfo)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_stateinfo_create(ref stateInfo));
    }

    internal static void SE_stateinfo_free(IntPtr stateInfo)
    {
      CApi.SE_stateinfo_free(stateInfo);
    }

    internal static void SE_stateinfo_get_id(IntPtr stateInfo, ref Int32 stateId)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_stateinfo_get_id(stateInfo, ref stateId));
    }

    internal static void SE_stateinfo_get_closing_time(IntPtr stateInfo, ref DateTime closing, out bool isNull)
    {
      Tm t = new Tm();
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_stateinfo_get_closing_time(stateInfo, ref t));

      isNull = t.IsNull();
      if (isNull == false)
      {
        closing = t.GetDate();
      }
    }

    #endregion

    internal static void SE_state_close(IntPtr connection, Int32 stateId)
    {
      ErrorHandling.CheckRC(connection, IntPtr.Zero,
        CApi.SE_state_close(connection, stateId));
    }

    public static void SE_state_open(IntPtr connection, Int32 stateId)
    {
      ErrorHandling.CheckRC(connection, IntPtr.Zero,
        CApi.SE_state_open(connection, stateId));
    }

    #endregion

    #region table

    internal static IntPtr[] GetPtrs(string[] names)
    {
      int nViewCols = names.Length;
      IntPtr[] ptrs = new IntPtr[nViewCols];
      for (int i = 0; i < nViewCols; i++)
      {
        ptrs[i] = Marshal.StringToHGlobalAnsi(names[i]);
      }
      return ptrs;
    }

    #endregion

    #region versioninfo

    internal static void SE_versioninfo_create(ref IntPtr versionInfo)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_versioninfo_create(ref versionInfo));
    }

    internal static void SE_versioninfo_free(IntPtr versionInfo)
    {
      CApi.SE_versioninfo_free(versionInfo);
    }

    internal static void SE_versioninfo_get_state_id(IntPtr versionInfo, ref Int32 stateId)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_versioninfo_get_state_id(versionInfo, ref stateId));
    }

    public static void SE_versioninfo_set_state_id(IntPtr versionInfo, Int32 stateId)
    {
      ErrorHandling.CheckRC(IntPtr.Zero, IntPtr.Zero,
        CApi.SE_versioninfo_set_state_id(versionInfo, stateId));
    }

    #endregion
  }
}
