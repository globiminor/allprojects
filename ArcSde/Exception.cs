namespace ArcSde
{
	//-------------------------------------------------------------------------
	/// <summary>
	/// Purpose: Allgemeine Fehlermeldung bei der Interaktion mit ArcSde<br/>
	/// Notes:	Die in ArcSde generierten Fehlerangaben werden in den managed
	///		Code übermittelt<br/>
	/// History: ML 02.09.2003 initial coding
	/// </summary>
	//-------------------------------------------------------------------------
	public class SdeException: System.Exception
	{
		//-------------------------------------------------------------------------
		/// <summary>
		/// Purpose: Initialisierung
		/// Notes:	<br/>
		/// History: ML 02.09.2003 initial coding
		/// </summary>
		/// <param name="errorMessage">Enthält die in ArcSDE generierte Fehlermeldung</param>
		/// <param name="source">Aufrufende Methode</param>
		//-------------------------------------------------------------------------
		public SdeException(string errorMessage, string source) :
			base(errorMessage)
		{
			  base.Source = source;
		}
	}
}
