using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyanStars.Dialogue
{
    public class VerticalDrawingSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject verticalDrawingPerfab;
        private void Start()
        {
            for (int i = 0; i < DialogueManager.Instance.dialogueContentCells[0].verticalDrawings.Count; i++)
            {
                DialogueManager.Instance.verticalDrawingBases.Add(Instantiate(verticalDrawingPerfab, transform, gameObject).GetComponent<VerticalDrawingBase>());
                DialogueManager.Instance.verticalDrawingBases[i].verticalDrawingID = i;
            }
        }
    }
}
