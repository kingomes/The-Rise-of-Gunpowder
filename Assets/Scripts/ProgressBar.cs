using UnityEngine;
using System.Collections;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] GameObject progress;

    public void SetProgress(float progressNormalized) {
        progress.transform.localScale = new Vector3(progressNormalized, 1f);
    }

    public IEnumerator SetProgressSmooth(float newProgress) {
        float curProgress = progress.transform.localScale.x;
        float changeAmt = curProgress - newProgress;

        while (curProgress - newProgress > Mathf.Epsilon) {
            curProgress -= changeAmt * Time.deltaTime;
            progress.transform.localScale = new Vector3(curProgress, 1f);
            yield return null;
        }

        progress.transform.localScale = new Vector3(newProgress, 1f);
    }
}
