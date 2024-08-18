namespace TeacherAssistant.GptApi.Models;

public class GptRequestDto
{

    public string Model { get; set; } = "gpt-4o-2024-05-13";
    public IEnumerable<GptMessageDto> Messages { get; set; }
    
}

public class GptMessageDto
{
    public string Role { get; set; }
    public string Content { get; set; }
}