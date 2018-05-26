using System;
using System.Text;

namespace ArcSde
{
	//-------------------------------------------------------------------------
	/// <summary>
	/// Purpose: Ermöglicht Zuordnung des "Return Code" von ArcSDE 8.3 C-API
	///		Funktion zu Aussagekräftigen Fehlermeldungen. Dabei wird<br/>
	///		nötigenfalls eine ArcSdeException geworfen.<br/>  
	/// History: ML 03.09.2003 initial coding
	/// </summary>
	//-------------------------------------------------------------------------
	public class ErrorHandling
	{
    public static bool IsNoError(int rc, out string errorMsg)
    {
      bool r = true;
      errorMsg = null;
      if ((rc != SdeErrNo.SE_SUCCESS) && (rc != SdeErrNo.SE_FINISHED))
      {
        r = false;

        char[] sGeneralError = new char[SdeType.SE_MAX_MESSAGE_LENGTH];
        CApi.SE_error_get_string(rc, sGeneralError);

        int i = 0;
        while (sGeneralError[i] != 0 && i < SdeType.SE_MAX_MESSAGE_LENGTH)
        {
          i++;
        }
        StringBuilder s = new StringBuilder();
        s.Insert(0, sGeneralError);

        errorMsg = s.ToString(0, i);
      }
      return r;
    }

		//-------------------------------------------------------------------------
		/// <summary>
		/// Purpose: Prüft Rückgabewert auf "Return Code".<br/>
		/// Notes:	 <br/>
		/// History: ML 03.09.2003 initial coding
		/// </summary>
		/// <param name="conn">Connection falls benutzt -> Prüfung Verbindung</param>
		/// <param name="stream">Stream falls benutzt -> Prüfung Datenstrom</param>
		/// <param name="rc">Return Code aus ArcSDE (Error code)</param>
		/// <exception cref="SdeException"></exception>
		//-------------------------------------------------------------------------
		public static void CheckRC(IntPtr conn, IntPtr stream, Int32 rc) 
		{ 
			if ( (rc != SdeErrNo.SE_SUCCESS) && (rc != SdeErrNo.SE_FINISHED) ) 
			{ 
        if (rc == SdeErrNo.SE_NET_FAILURE && conn != IntPtr.Zero)
        {
          Se_Error error = new Se_Error();
          int rrc = CApi.SE_connection_reconnect(conn, ref error);
        }
				string sErrorMessage;
				char[] sGeneralError = new char[SdeType.SE_MAX_MESSAGE_LENGTH]; 
				CApi.SE_error_get_string(rc, sGeneralError);


				StringBuilder helper = new StringBuilder();
				helper.Insert(0, sGeneralError);
				
				sErrorMessage = helper.ToString();
				sErrorMessage = sErrorMessage.TrimEnd('\0');

				// Print extended error info, if any
				if((SdeErrNo.SE_DB_IO_ERROR == rc) | (SdeErrNo.SE_INVALID_WHERE == rc))	
				{ 
					Int32 iTemp_rc;
					Se_Error pError = new Se_Error();
					
					if(conn == IntPtr.Zero)
					{ 
						// Assume this is a stream error
						iTemp_rc = CApi.SE_stream_get_ext_error(stream, ref pError); 
					} 
					else	
					{ 
						// Assume this is a connection error 
						iTemp_rc = CApi.SE_connection_get_ext_error(conn, ref pError); 
					} 
					if (SdeErrNo.SE_SUCCESS == iTemp_rc)	
					{
						sErrorMessage += 
							"Extended error code: %d, extended error string:\n%s\n" + 
							pError.ext_error + pError.err_msg1;
						//printf ("Extended error code: %d, extended error string:\n%s\n", error.ext_error, error.err_msg1); 
					} 
				}
				throw new SdeException(sErrorMessage
					+ ". Return Code: " + rc,System.Reflection.MethodBase.GetCurrentMethod().Name);
			}
		} 
	}
}
