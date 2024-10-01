namespace TelemarketingControlSystem.Models
{
	public class ProjectType
	{
		public int Id { get; set; }
		public string Name { get; set; }

		public List<Project> Projects { get; set; }
		public List<TypeDictionary> TypeDictionaries { get; set; }
		public List<TypeMistakeDictionary> TypeMistakeDictionaries { get; set; }
	}
}
