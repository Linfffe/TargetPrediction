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

		//offestT�ļ���˳���ڼ�������ʱ�����ˣ��Ͼ���������
		//calculateOffestT();
	}

	//����Ŀ��λ�ü�������,Ŀ��λ��������Ԥ��λ��
	float calculateAngle_up(Vector3 position) {

		Vector3 toTarget = position - muzzle.position;

		//��ά����e����x��f����y��
		float e = Mathf.Sqrt(toTarget.x * toTarget.x + toTarget.z * toTarget.z);
		float f = toTarget.y;

		float temp = (shellG * e * e)/ (shellVelocity * shellVelocity) - f;
		temp /= Mathf.Sqrt(f * f + e * e);

		temp = Mathf.Clamp(temp, -1, 1);
		float a = (Mathf.Asin(temp) - Mathf.Atan(f/e)) / 2;


		//---------------offestT--------------------------
			//��ȷ���㵱ǰ�ڵ�����ǰԤ��λ�õ�ʱ��
		float t = e / (shellVelocity * Mathf.Cos(a));

		if (t >= currentT) offestT += 0.01f;
		else offestT -= 0.01f;

		//�޲���������ν
		offestT = Mathf.Clamp(offestT, 0, 10);

		//Debug.Log(offestT);
		//---------------offestT--------------------------

		return Mathf.Atan(Mathf.Cos(a)/Mathf.Sin(a))/Mathf.PI*180+90;
	}

	//����Ŀ��λ�ü���������ת,Ŀ��λ��������Ԥ��λ��
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

	//Ԥ��λ��
	Vector3 predicPosition() {
		targetVelocity = target.GetComponent<Rigidbody>().velocity;

		if (targetVelocity.magnitude <= 0.1f) return target.position;

		//�����Ǻ�������
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

	// offestT�ļ���˳���ڼ�������ʱ�����ˣ��Ͼ���������
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
