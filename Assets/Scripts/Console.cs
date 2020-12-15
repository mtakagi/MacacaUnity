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
        var sb = new System.Text.StringBuilder(mInputField.text);

        for (var token = lexer.NextToken(); token.Type != Macaca.TokenType.EOF; token = lexer.NextToken())
        {
            sb.Append($"\nType: {token.Type}, Literal: {token.Literal}");
        }

        mInputField.text = sb.ToString();
    }
}
