using Codeer.LowCode.Blazor.OperatingModel;
using Codeer.LowCode.Blazor.ProCode;

namespace WebApp.Client.Shared.Samples
{
    public class CodeBehindSample : ProCodeBehindBase
    {
        public TextField? Name { get; set; }
        public NumberField? Age { get; set; }
        public ButtonField? OK { get; set; }
        public TextField? Result { get; set; }

        public override async Task OnBeforeDetailInitializationAsync(string layoutName)
        {
            await Task.CompletedTask;
            if (OK != null)OK.OnClickAsync = OK_Click;
        }

        private async Task OK_Click()
        {
            var value = @$"This is a sample code-behind.
This string is created using C# code.
Implemented in {GetType().FullName}.
Your name is {Name?.Value}.
And I'm {Age?.Value} years old.";

            if (Result != null) await Result.SetValueAsync(value);
        }

    }
}
