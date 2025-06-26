using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class NumbersOnlyTMPInput : MonoBehaviour
{
    private TMP_InputField inputField;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValidateInput += ValidateInput;
    }

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        if (char.IsDigit(addedChar))
        {
            return addedChar;
        }
        return '\0'; // Reject character
    }
}
