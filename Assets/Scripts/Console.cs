using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour
{
    [SerializeField]
    private InputField mInputField;

    void Start()
    {
        mInputField.text = "";
        mInputField.ActivateInputField();
    }

    public void REPL()
    {
        var lexer = new Macaca.Lexer(mInputField.text);
        var parser = new Macaca.Parser(lexer);
        var sb = new System.Text.StringBuilder(mInputField.text);

        sb.Append('\n');
        sb.Append(parser.ParseProgram().String);
        sb.Append('\n');

        mInputField.text = sb.ToString();
    }
}
