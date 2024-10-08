namespace TelemarketingControlSystem.Models
{
	public class ProjectType : BaseModel
	{
		public string Name { get; set; }

		public List<Project> Projects { get; set; }
		public List<ProjectTypeDictionary> ProjectTypeDictionaries { get; set; }
		public List<ProjectTypeMistakeDictionary> ProjectTypeMistakeDictionaries { get; set; }
	}
}
