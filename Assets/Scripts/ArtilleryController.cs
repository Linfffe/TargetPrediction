using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ArtilleryController : MonoBehaviour
{
	public Transform target;
	public Transform shell;

	public float upAngle, rightAngle;

	private Vector3 targetVelocity;
	private Transform body;
	private Transform muzzle;
	private float shellVelocity;
	private float shellG;
	private float beta;
	private float distance;
	private float offestT;


	private float currentT;
	private Vector3 currentPredicPosition;

	// Start is called before the first frame update
	void Start()
    {
		body = transform.GetChild(0);
		muzzle = body.GetChild(0).GetChild(0);
		shellVelocity = shell.GetComponent<ShellScript>().initVelocity;
		shellG = shell.GetComponent<ShellScript>().G;

		StartCoroutine(spawnManager());
    }

    // Update is called once per frame
    void Update()
    {
		updataDirection();

		//offestT的计算顺便在计算仰角时计算了，毕竟代码相似
		//calculateOffestT();
	}

	//根据目标位置计算仰角,目标位置往往是预测位置
	float calculateAngle_up(Vector3 position) {

		Vector3 toTarget = position - muzzle.position;

		//二维化，e就是x，f就是y；
		float e = Mathf.Sqrt(toTarget.x * toTarget.x + toTarget.z * toTarget.z);
		float f = toTarget.y;

		float temp = (shellG * e * e)/ (shellVelocity * shellVelocity) - f;
		temp /= Mathf.Sqrt(f * f + e * e);

		temp = Mathf.Clamp(temp, -1, 1);
		float a = (Mathf.Asin(temp) - Mathf.Atan(f/e)) / 2;


		//---------------offestT--------------------------
			//精确计算当前炮弹到当前预测位置的时间
		float t = e / (shellVelocity * Mathf.Cos(a));

		if (t >= currentT) offestT += 0.01f;
		else offestT -= 0.01f;

		//限不限制无所谓
		offestT = Mathf.Clamp(offestT, 0, 10);

		//Debug.Log(offestT);
		//---------------offestT--------------------------

		return Mathf.Atan(Mathf.Cos(a)/Mathf.Sin(a))/Mathf.PI*180+90;
	}

	//根据目标位置计算左右旋转,目标位置往往是预测位置
	float calculateAngle_right(Vector3 position) {
		Vector3 direction = position - body.position;
		Vector2 temp = new Vector2(direction.x, direction.z).normalized;
		rightAngle = Vector2.SignedAngle(new Vector2(1, 0), temp);

		return rightAngle;
	}

	void updataDirection() {
		setAngle_right(calculateAngle_right(predicPosition()));
		setAngle_up(calculateAngle_up(predicPosition()));
	}

	//预测位置
	Vector3 predicPosition() {
		targetVelocity = target.GetComponent<Rigidbody>().velocity;

		if (targetVelocity.magnitude <= 0.1f) return target.position;

		//解三角函数方程
		beta = shellVelocity / targetVelocity.magnitude;
		distance = Vector3.Distance(target.position, muzzle.position);
		float cosB = Vector3.Dot((target.position-muzzle.position ).normalized, targetVelocity.normalized);
		float x = 2 * cosB * distance + Mathf.Sqrt((2 * cosB * distance) * ((2 * cosB * distance)) - 4 * (1 - beta * beta) * distance * distance);
		x /= -2*(1-beta*beta);

		currentT = x / targetVelocity.magnitude+offestT;
		//Debug.DrawLine(target.position, target.position + targetVelocity * (currentT)  ,Color.red,0.1f);
		currentPredicPosition = target.position + targetVelocity * (currentT);
		return currentPredicPosition;
	}

	// offestT的计算顺便在计算仰角时计算了，毕竟代码相似
	void calculateOffestT() {

		Vector3 toTarget = currentPredicPosition - muzzle.position;

		float e = Mathf.Sqrt(toTarget.x * toTarget.x + toTarget.z * toTarget.z);
		float f = toTarget.y;

		float temp = (shellG * e * e) / (shellVelocity * shellVelocity) - f;
		temp /= Mathf.Sqrt(f * f + e * e);

		float a = (Mathf.Asin(temp) - Mathf.Atan(f / e)) / 2;

		float t = e /(shellVelocity * Mathf.Cos(a));

		if (t > currentT) offestT += 0.1f;
		else offestT -= 0.1f;

		offestT = Mathf.Clamp(offestT, -3, 3);

		Debug.Log(offestT);
	}

	void setAngle_up(float angle) {
		body.eulerAngles = new Vector3(body.eulerAngles.x, body.eulerAngles.y, -angle);
	}
	void setAngle_right(float angle) {
		transform.eulerAngles = new Vector3(transform.eulerAngles.x, 180-angle, transform.eulerAngles.z);
	}

	void spawnShell() {
		Transform temp = Instantiate(shell);
		temp.position = muzzle.position;
		temp.eulerAngles = muzzle.eulerAngles;
	}
	IEnumerator spawnManager() {
		while (true) {
			spawnShell();
			yield return new WaitForSeconds(0.5f);
		}
	}

}
