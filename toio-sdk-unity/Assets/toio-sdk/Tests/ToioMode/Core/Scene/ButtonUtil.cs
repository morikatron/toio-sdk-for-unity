using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace toio.Tests.Core
{
    public class ButtonUtil : MonoBehaviour
    {
        public void OnOffActive()
        {
            this.gameObject.SetActive(!this.gameObject.activeSelf);
        }
    }
}

