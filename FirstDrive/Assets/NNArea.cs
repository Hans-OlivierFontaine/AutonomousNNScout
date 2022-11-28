using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(DecisionRequester))]
public class NNArea : Agent
{
    [System.Serializable]
    public class RewardInfo
    {
        public float mult_forward = 0.001f;
        public float barrier = -0.001f;
        public float car = -0.001f;
    }

    public float movespeed = 3f;
    public float turnspeed = 1f;
    public RewardInfo rwd = new RewardInfo();
    private Rigidbody rigidbody = null;
    private Vector3 recall_position;
    private Quaternion recall_rotation;
    private Bounds bnd;
    // Start is called before the first frame update
    public override void Initialize()
    {
        rigidbody = this.GetComponent<Rigidbody>();
        rigidbody.drag = 1;
        rigidbody.angularDrag = 5;
        rigidbody.interpolation = RigidbodyInterpolation.Extrapolate;

        this.GetComponent<MeshCollider>().convex = true;
        this.GetComponent<DecisionRequester>().DecisionPeriod = 1;
        bnd = this.GetComponent<MeshRenderer>().bounds;

        recall_position = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        //recall_position = new Vector3(45, 0.5f, 45);
        recall_rotation = new Quaternion(this.transform.rotation.x, this.transform.rotation.y, this.transform.rotation.z, this.transform.rotation.w);
    }

    public override void OnEpisodeBegin()
    {
        rigidbody.velocity = Vector3.zero;
        this.transform.position = recall_position;
        this.transform.rotation = recall_rotation;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        System.Console.WriteLine("ActionReceived");
        if (isWheelsDown() == false)
            return;
        
        float mag = rigidbody.velocity.sqrMagnitude;
        switch (actions.DiscreteActions.Array[0])
        {
            case 0:
                break;
            case 1:
                rigidbody.AddRelativeForce(Vector3.back * movespeed * Time.deltaTime, ForceMode.VelocityChange);
                break;
            case 2:
                rigidbody.AddRelativeForce(Vector3.forward * movespeed * Time.deltaTime, ForceMode.VelocityChange);
                AddReward(mag * rwd.mult_forward);
                break;
        }

        switch (actions.DiscreteActions.Array[1])
        {
            case 0:
                break;
            case 1:
                this.transform.Rotate(Vector3.up, -turnspeed * Time.deltaTime);
                break;
            case 2:
                this.transform.Rotate(Vector3.up, turnspeed * Time.deltaTime);
                break;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.DiscreteActions.Array[0] = 0;
        actionsOut.DiscreteActions.Array[1] = 0;

        float move = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        if (move < 0)
            actionsOut.DiscreteActions.Array[0] = 1;
        else if (move > 0)
            actionsOut.DiscreteActions.Array[0] = 2;

        if (turn < 0)
            actionsOut.DiscreteActions.Array[1] = 1;
        else if (turn > 0)
            actionsOut.DiscreteActions.Array[1] = 2;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wallBlack") == true || collision.gameObject.CompareTag("wallColor") == true)
        {
            AddReward(rwd.barrier);
        }else if (collision.gameObject.CompareTag("car") == true){
            AddReward(rwd.car);
        }
    }

    private bool isWheelsDown()
    {
        return Physics.Raycast(this.transform.position, -this.transform.up, bnd.size.y * 0.55f);
    }
}
