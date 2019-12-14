using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class LineFeedFixed : BaseMeshEffect
{
    static readonly HashSet<char> charset = new HashSet<char>(new char[] { ',',';','.','?','!','，','。','；','?','！',')','”','’','）','》' });
    private Text textCompent;
    public bool moveUpMode = true;

    protected override void OnEnable()
    {
        base.OnEnable();
        if (textCompent == null)
            textCompent = GetComponent<Text>();
    }

    private static List<UIVertex> output = new List<UIVertex>();
    private static List<UILineInfo> lines = new List<UILineInfo>();
    private static List<UICharInfo> characters = new List<UICharInfo>();

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        vh.GetUIVertexStream(output);

        Vector2 alignment = Text.GetTextAnchorPivot(textCompent.alignment);

        string text = textCompent.text;
        textCompent.cachedTextGenerator.GetLines(lines);
        textCompent.cachedTextGenerator.GetCharacters(characters);

        for (int i = 0;i < lines.Count;i++)
        {
            var line = lines[i];
            if (i > 0 && charset.Contains(text[line.startCharIdx]))
            {
                var character = characters[line.startCharIdx];
                var preCharacter = characters[line.startCharIdx - 1];
                var preLine = lines[i - 1];
                int lineEndIdx = i < lines.Count - 1 ? lines[i + 1].startCharIdx : text.Length;
                if (moveUpMode)
                {
                    MoveChars(line.startCharIdx, line.startCharIdx + 1, preCharacter.cursorPos + new Vector2(character.charWidth, 0f) - character.cursorPos);
                    MoveChars(preLine.startCharIdx, line.startCharIdx + 1, new Vector2(-character.charWidth, 0) * alignment);
                    MoveChars(line.startCharIdx + 1, lineEndIdx, new Vector2(-character.charWidth * (1 - alignment.x), 0));
                }
                else
                {
                    MoveChars(line.startCharIdx - 1, line.startCharIdx, character.cursorPos - new Vector2(preCharacter.charWidth, 0f) - preCharacter.cursorPos);
                    MoveChars(preLine.startCharIdx, line.startCharIdx - 1, new Vector2(preCharacter.charWidth, 0) * alignment);
                    MoveChars(line.startCharIdx - 1, lineEndIdx, new Vector2(preCharacter.charWidth * (1 - alignment.x), 0));
                }
            }
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(output);
    }

    void MoveChars(int start,int end, Vector3 offest)
    {
        for (int index = start; index < end; index++)
        {
            int i = index * 6;
            UIVertex v1 = output[i];
            UIVertex v2 = output[i + 1];
            UIVertex v3 = output[i + 2];
            UIVertex v4 = output[i + 3];
            UIVertex v5 = output[i + 4];
            UIVertex v6 = output[i + 5];

            v1.position += offest;
            v2.position += offest;
            v3.position += offest;
            v4.position += offest;
            v5.position += offest;
            v6.position += offest;

            output[i] = v1;
            output[i + 1] = v2;
            output[i + 2] = v3;
            output[i + 3] = v4;
            output[i + 4] = v5;
            output[i + 5] = v6;
        }
    }
}


