using TMPro;
using UnityEngine;

public struct SteeringOutput
{
    public Vector2 LinearVelocity;
    public bool AutoOrient;
    public bool ImmediateStop;
}

[RequireComponent(typeof(Rigidbody2D))]
public class UnitMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject m_TargetLocationIndicator;

    public Vector2 TargetPosition
    {
        get { return m_TargetPosition; }
        set
        {
            m_TargetPosition = value;
            if (m_UnitStats.Faction == GameManager.Instance.PlayerFaction)
                m_TargetLocationIndicator.SetActive(true);
            m_ReachedTarget = false;
        }
    }
    public Vector2 Velocity => m_Velocity;

    private BaseSteeringBehavior[] m_SteeringBehaviours;
    private UnitStats m_UnitStats;
    private Rigidbody2D m_Body;

    private Vector2 m_TargetPosition;
    private Vector2 m_Velocity;
    private bool m_ReachedTarget;

    public T GetSteeringBehaviour<T>()
    {
        foreach (var behaviour in m_SteeringBehaviours)
        {
            if (behaviour is T typedBehaviour)
            {
                return typedBehaviour;
            }
        }
        return default;
    }

    private void Start()
    {
        m_UnitStats = GetComponent<UnitStats>();
        m_Body = GetComponent<Rigidbody2D>();
        InitializeSteeringBehaviours();

        TargetPosition = transform.position;
        m_TargetLocationIndicator.SetActive(false);

        GetSteeringBehaviour<Flee>().Enabled = false;

        m_UnitStats.OnMoraleBroken.AddListener(() =>
        {
            GetSteeringBehaviour<Arrive>().Enabled = false;
            GetSteeringBehaviour<Flee>().Enabled = true;
        });

        m_UnitStats.OnMoraleFixed.AddListener(() =>
        {
            GetSteeringBehaviour<Arrive>().Enabled = true;
            GetSteeringBehaviour<Flee>().Enabled = false;
        });
    }

    private void FixedUpdate()
    {
        PerformSteerings();
    }

    private void PerformSteerings()
    {
        if (m_ReachedTarget)
        {
            m_Velocity = Vector2.zero;
            return;
        }

        SteeringOutput finalOutput = new SteeringOutput
        {
            LinearVelocity = Vector2.zero,
            AutoOrient = false
        };

        foreach (BaseSteeringBehavior steering in m_SteeringBehaviours)
        {
            if (!steering.Enabled) continue;

            var output = steering.GetSteering(m_UnitStats, this, TargetPosition);
            finalOutput.LinearVelocity += output.LinearVelocity;
            finalOutput.AutoOrient |= output.AutoOrient;
            finalOutput.ImmediateStop |= output.ImmediateStop;
        }


        finalOutput.LinearVelocity = Vector2.ClampMagnitude(finalOutput.LinearVelocity, m_UnitStats.Acceleration);


        if (!finalOutput.ImmediateStop)
        {

            m_Velocity += finalOutput.LinearVelocity * Time.fixedDeltaTime;


            float brakingFactor = 5f;
            m_Velocity = Vector2.Lerp(m_Velocity, Vector2.zero, brakingFactor * Time.fixedDeltaTime);


            m_Velocity = Vector2.ClampMagnitude(m_Velocity, m_UnitStats.MaxMovementSpeed);
        }
        else
        {
            m_Velocity = Vector2.zero;
        }


        float distance = Vector2.Distance(transform.position, TargetPosition);
        if (distance < 0.5f || finalOutput.ImmediateStop)
        {
            m_ReachedTarget = true;
            if (m_UnitStats.Faction == GameManager.Instance.PlayerFaction)
                m_TargetLocationIndicator.SetActive(false);
            m_Velocity = Vector2.zero;
            return;
        }
        else
        {
            if (m_UnitStats.Faction == GameManager.Instance.PlayerFaction)
                m_TargetLocationIndicator.transform.position = TargetPosition;
        }


        m_Body.MovePosition(transform.position + (Vector3)m_Velocity * Time.fixedDeltaTime);


        if (finalOutput.AutoOrient && Mathf.Abs(m_Velocity.x) > .5f)
        {
            var localScale = transform.localScale;
            localScale.x = m_Velocity.x > 0 ? 1 : -1;
            transform.localScale = localScale;
        }
    }


    private Vector2 CaculateSteering(SteeringOutput steeringOutput, out bool isBraking)
    {
        if (steeringOutput.LinearVelocity == Vector2.zero)
        {
            isBraking = true;
            return -m_Velocity.normalized * m_UnitStats.Decceleration;
        }
        else
        {
            isBraking = false;
            return steeringOutput.LinearVelocity - m_Velocity;
        }
    }

    private void InitializeSteeringBehaviours()
    {
        m_SteeringBehaviours = new BaseSteeringBehavior[]
        {
            new Arrive(),
            new Seperation(),
            new Flee()
        };
    }

    private void OnDrawGizmosSelected()
    {
        if (m_SteeringBehaviours != null)
        {
            foreach (var steeringBehaviour in m_SteeringBehaviours)
            {
                steeringBehaviour.DrawGizmos(m_UnitStats, TargetPosition);
            }
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(TargetPosition, 0.5f); // Draw a small sphere at the target position
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)Velocity);

    }
}

public interface ISteeringBehavior
{
    public SteeringOutput GetSteering(UnitStats unit, UnitMovement unitMovement, Vector2 targetPosition);
    public void DrawGizmos(UnitStats unit, Vector2 targetPosition);
}

public class BaseSteeringBehavior : ISteeringBehavior
{
    public bool Enabled { get; set; }

    public BaseSteeringBehavior()
    {
        Enabled = true;
    }

    public virtual SteeringOutput GetSteering(UnitStats unit, UnitMovement unitMovement, Vector2 targetPosition)
    {
        return new SteeringOutput();
    }

    public virtual void DrawGizmos(UnitStats unit, Vector2 targetPosition)
    {
        // Default implementation does nothing
    }
}

public class Arrive : BaseSteeringBehavior
{
    public float ArrivalRadius = 0.25f;
    public float SlowRadius = 1.5f;

    public override SteeringOutput GetSteering(UnitStats unit, UnitMovement unitMovement, Vector2 targetPosition)
    {
        SteeringOutput output = new SteeringOutput();

        Vector2 direction = targetPosition - (Vector2)unit.transform.position;
        float distance = direction.magnitude;

        if (distance < ArrivalRadius)
        {
            output.ImmediateStop = true;
            output.LinearVelocity = Vector2.zero;
        }
        else
        {
            float targetSpeed = unit.MaxMovementSpeed;

            if (distance < SlowRadius)
            {
                // Slow down proportionally as you approach
                targetSpeed *= (distance / SlowRadius);
            }

            Vector2 desiredVelocity = direction.normalized * targetSpeed;
            output.LinearVelocity = (desiredVelocity - unitMovement.Velocity).normalized * unit.Acceleration;

            output.AutoOrient = true;
        }

        return output;
    }

    public override void DrawGizmos(UnitStats unit, Vector2 targetPosition)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetPosition, SlowRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, ArrivalRadius);
    }
}

public class Seperation : BaseSteeringBehavior
{
    private float m_SeparationRadius;
    public Seperation(float separationRadius = 1.1f)
    {
        m_SeparationRadius = separationRadius;
    }

    public override void DrawGizmos(UnitStats unit, Vector2 targetPosition)
    {
    }

    public override SteeringOutput GetSteering(UnitStats unit, UnitMovement unitMovement, Vector2 targetPosition)
    {
        SteeringOutput output = new SteeringOutput();

        var neighbours = GameManager.Instance.GetUnitsInRadius(unit.Faction, unit.transform.position, m_SeparationRadius);

        Vector2 separationForce = Vector2.zero;
        foreach (var neighbour in neighbours)
        {
            if (neighbour == unit) continue;

            Vector2 directionToNeighbour = unit.transform.position - neighbour.transform.position;
            float distance = directionToNeighbour.magnitude;

            if (distance < m_SeparationRadius) 
            {
                separationForce += directionToNeighbour.normalized / distance; 
            }
        }
        if (separationForce != Vector2.zero)
        {
            output.LinearVelocity = separationForce.normalized * unit.Acceleration;
            output.AutoOrient = true;
        }
        else
        {
            output.LinearVelocity = Vector2.zero;
        }
        return output;
    }
}

public class Flee : BaseSteeringBehavior
{
    public float m_FleeRadius = 30f;

    public override SteeringOutput GetSteering(UnitStats unit, UnitMovement unitMovement, Vector2 targetPosition)
    {
        SteeringOutput output = new SteeringOutput();

        var neighbours = GameManager.Instance.GetUnitsInRadius(unit.Faction == Factions.Humans ? Factions.Orcs : Factions.Humans, unit.transform.position, m_FleeRadius);

        Vector2 separationForce = Vector2.zero;
        foreach (var neighbour in neighbours)
        {
            if (neighbour == unit) continue;

            Vector2 directionToNeighbour = unit.transform.position - neighbour.transform.position;
            float distance = directionToNeighbour.magnitude;

            if (distance < m_FleeRadius)
            {
                separationForce += directionToNeighbour.normalized / distance;
            }
        }
        if (separationForce != Vector2.zero)
        {
            output.LinearVelocity = separationForce.normalized * unit.Acceleration;
            output.AutoOrient = true;
        }
        else
        {
            output.LinearVelocity = Vector2.zero;
        }
        return output;
    }
}



