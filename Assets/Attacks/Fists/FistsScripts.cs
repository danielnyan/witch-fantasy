using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Animations.Fists
{
    public class FistsScripts : MonoBehaviour
    {
        public GameObject fireNova;
        public void BaseAttack3(GameObject o)
        {
            /*
            IEnumerator helper(GameObject c)
            {
                c.SetActive(true);
                yield return new WaitForSeconds(1);
                c.SetActive(false);
            }*/
            GameObject child = o.transform.Find("Fire Nova").gameObject;
            GameObject f = Instantiate(fireNova, child.transform);
            f.SetActive(false);
            f.transform.localPosition = Vector3.zero;
            f.transform.SetParent(null);
            f.SetActive(true);
            // StartCoroutine(helper(child));
        }
    }
}
