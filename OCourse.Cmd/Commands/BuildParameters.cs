
using Basics.Cmd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OCourse.Cmd.Commands
{
	public class BuildParameters : IDisposable
	{
		private static readonly List<Command<BuildParameters>> _cmds = new List<Command<BuildParameters>>
				{
						new Command<BuildParameters>
						{
								Key = "-c",
								Parameters = "<config path>",
								Read = (p, args, i) =>
								{
										p.ConfigPath = args[i + 1];
										return 1;
								}
						},

						new Command<BuildParameters>
						{
								Key = "-o",
								Parameters = "<output path>",
								Optional = false,
								Read = (p, args, i) =>
								{
										p.OutputPath = args[i + 1];
										return 1;
								}
						},


						new Command<BuildParameters>
						{
								Key = "-b",
								Parameters = "<Bahn>",
								Optional = false,
                //Default = ()=> new [] {SamaxContext.Instance.DefaultDb },
                Read = (p, args, i) =>
								{
										p.Course = args[i + 1];
										return 1;
								}
						},
						new Command<BuildParameters>
						{
								Key = "-s",
								Parameters = "<begin startNr>",
								Optional = true,
								Read = (p, args, i) =>
								{
										p.BeginStartNr = args[i + 1];
										return 1;
								}
						},
						new Command<BuildParameters>
						{
								Key = "-e",
								Parameters = "<end startNr>",
								Optional = true,
								Read = (p, args, i) =>
								{
									p.EndStartNr = args[i + 1];
									return 1;
								}
						},
						new Command<BuildParameters>
						{
								Key = "-p",
								Parameters = "(split courses)",
								Optional = true,
                Read = (p, args, i) =>
								{
										p.SplitParameters = new Ext.SplitBuilder.Parameters();
										return 0;
								}
						},
				};

		public string ConfigPath
		{
			get { return _configPath; }
			set { _configPath = value; _oCourseVm = null; }
		}

		private string _configPath;
		public string OutputPath { get; set; }
		public string TemplatePath { get; set; }
		public string Course { get; set; }
		public string BeginStartNr { get; set; }
		public string EndStartNr { get; set; }
		public Ext.SplitBuilder.Parameters SplitParameters { get; set; }
		public bool UseAllPermutations { get; set; }

		private ViewModels.OCourseVm _oCourseVm;
		public ViewModels.OCourseVm OCourseVm => _oCourseVm;
		public ViewModels.OCourseVm EnsureOCourseVm()
		{
			if (_oCourseVm == null)
			{
				if (_oCourseVm == null)
				{
					ViewModels.OCourseVm oCourseVm = new ViewModels.OCourseVm();
					oCourseVm.LoadSettings(ConfigPath);
					oCourseVm.RunInSynch = true;

					_oCourseVm = oCourseVm;
				}
			}
			return _oCourseVm;
		}
		public void Dispose()
		{
			_oCourseVm?.Dispose();
			_oCourseVm = null;
		}
		public string Validate()
		{
			StringBuilder sb = new StringBuilder();


			int beginStartNr = -1;
			if (!string.IsNullOrEmpty(BeginStartNr) && !int.TryParse(BeginStartNr, out beginStartNr))
			{ sb.AppendLine($"Invalid BeginStartNr '{BeginStartNr}'"); }

			int endStartNr = -1;
			if (!string.IsNullOrEmpty(EndStartNr) && !int.TryParse(EndStartNr, out endStartNr))
			{ sb.AppendLine($"Invalid EndStartNr '{EndStartNr}'"); }

			if (!File.Exists(ConfigPath))
			{ sb.AppendLine($"Unknown config File '{ConfigPath}'"); }
			else
			{
				try
				{
					EnsureOCourseVm();
					//_oCourseVm?.Dispose();
					ViewModels.OCourseVm vm = _oCourseVm;


					if (!vm.CourseNames.Contains(Course))
					{
						sb.AppendLine($"Unknown course '{Course}'");
						sb.AppendLine($"Available courses: {string.Concat(vm.CourseNames.Select(x => $"{x},"))}");
					}

					vm.VarBuilderType = ViewModels.VarBuilderType.All;
					vm.CourseName = Course;
					while (vm.Working)
					{ System.Threading.Thread.Sleep(100); }
					if (beginStartNr > 0)
					{ vm.StartNrMin = beginStartNr; }
					if (endStartNr > 0)
					{ vm.StartNrMax = endStartNr; }

					if (vm.StartNrMin <= 0)
					{ sb.AppendLine($"Invalid start nr Min '{vm.StartNrMin}'"); }
					if (vm.StartNrMax < vm.StartNrMin)
					{ sb.AppendLine($"Invalid start nr range '[{vm.StartNrMin} - {vm.StartNrMax}]'"); }

				}
				catch (Exception e)
				{
					sb.AppendLine($"Invalid config File '{ConfigPath}' : {Basics.Utils.GetMsg(e)}");
					_oCourseVm.Dispose();
					_oCourseVm = null;
				}
			}

			string error = sb.ToString();
			return error;
		}
		/// <summary>
		///     interpret and verify command line arguments
		/// </summary>
		/// <param name="args"></param>
		/// <returns>null, if not successfull, otherwise interpreted parameters</returns>
		public static ICommand ReadArgs(IList<string> args)
		{
			BuildParameters result = new BuildParameters();

			if (Basics.Cmd.Utils.ReadArgs(result, args, _cmds, out string error))
			{
				if (string.IsNullOrWhiteSpace(error))
				{
					error = result.Validate();
				}
			}

			if (!string.IsNullOrWhiteSpace(error))
			{
				Console.WriteLine(error);
				return null;
			}

			return new BuildCommand(result);
		}
	}

}
