


#include "Vehicle/Vehicle.h"
#include "Kismet/GameplayStatics.h"
#include "EnhancedInputComponent.h"
#include "EnhancedInputSubsystems.h"
#include "InputMappingContext.h"
#include "GameFramework/SpringArmComponent.h"
#include "Camera/CameraComponent.h"
#include "NiagaraComponent.h"

AVehicle::AVehicle()
{
	PrimaryActorTick.bCanEverTick = true;

	CarBodyMesh = CreateDefaultSubobject<UStaticMeshComponent>("CarBodyMesh");
	RootComponent = CarBodyMesh;
	CarBodyMesh->SetSimulatePhysics(true);
	CarBodyMesh->SetMassOverrideInKg(FName(), 10000.0f);
	CarBodyMesh->SetLinearDamping(0.0f);
	CarBodyMesh->SetAngularDamping(10.0f);

	SpringArm = CreateDefaultSubobject<USpringArmComponent>("SpringArm");
	SpringArm->SetupAttachment(RootComponent);

	Camera = CreateDefaultSubobject<UCameraComponent>("Camera");
	Camera->SetupAttachment(SpringArm, USpringArmComponent::SocketName);

	AccelerationPoint = CreateDefaultSubobject<USceneComponent>("AccelerationPoint");
	AccelerationPoint->SetupAttachment(CarBodyMesh);

	Wheels.SetNum(4);
	Springs.SetNum(4);
	TireMeshes.SetNum(4);
	WheelIsGrounded.SetNum(4);

	FName WheelNames[4] = { "Wheel_LF", "Wheel_RF", "Wheel_LB", "Wheel_RB" };
	FName SpringNames[4] = { "Spring_LF", "Spring_RF", "Spring_LB", "Spring_RB" };
	FName TireNames[4] = { "Tire_LF", "Tire_RF", "Tire_LB", "Tire_RB" };

	for (int32 i = 0; i < 4; i++)
	{
		Wheels[i] = CreateDefaultSubobject<USceneComponent>(WheelNames[i]);
		Wheels[i]->SetupAttachment(RootComponent);

		Springs[i] = CreateDefaultSubobject<USceneComponent>(SpringNames[i]);
		Springs[i]->SetupAttachment(Wheels[i]);

		TireMeshes[i] = CreateDefaultSubobject<UStaticMeshComponent>(TireNames[i]);
		TireMeshes[i]->SetupAttachment(Wheels[i]);
		TireMeshes[i]->SetCollisionEnabled(ECollisionEnabled::NoCollision);

		WheelIsGrounded[i] = false;
	}

	FName SkidMarkNames[2] = { "SkidMark_LB", "SkidMark_RB"};
	BreakSkidMarks.SetNum(2);
	for (int32 i = 0; i < 2; i++)
	{
		BreakSkidMarks[i] = CreateDefaultSubobject<UNiagaraComponent>(SkidMarkNames[i]);
		BreakSkidMarks[i]->SetupAttachment(Wheels[i + 2]);
	}
}

void AVehicle::BeginPlay()
{
	Super::BeginPlay();
	
	if (APlayerController* PlayerController = Cast<APlayerController>(GetController()))
	{
		if (UEnhancedInputLocalPlayerSubsystem* Subsystem = ULocalPlayer::GetSubsystem<UEnhancedInputLocalPlayerSubsystem>(PlayerController->GetLocalPlayer()))
		{
			Subsystem->AddMappingContext(VehicleContext, 0);
		}
	}
}

void AVehicle::Tick(float DeltaTime)
{
	Super::Tick(DeltaTime);

	Suspension();
	GroundCheck();
	CalculateCarVelocity();
	Movement();
	Visuals();
}

void AVehicle::SetupPlayerInputComponent(UInputComponent* PlayerInputComponent)
{
	Super::SetupPlayerInputComponent(PlayerInputComponent);

	if (UEnhancedInputComponent* EnhancedInputComponent = Cast<UEnhancedInputComponent>(PlayerInputComponent))
	{
		EnhancedInputComponent->BindAction(MoveAction, ETriggerEvent::Triggered, this, &AVehicle::MoveActionInput);
		EnhancedInputComponent->BindAction(MoveAction, ETriggerEvent::Completed, this, &AVehicle::MoveActionInput);
		EnhancedInputComponent->BindAction(BreakAction, ETriggerEvent::Triggered, this, &AVehicle::BreakActionInput);
		EnhancedInputComponent->BindAction(BreakAction, ETriggerEvent::Completed, this, &AVehicle::BreakActionInput);
	}
}

void AVehicle::Suspension()
{
	FHitResult Hit;
	float MaxLength = RestLength + SpringTravel;

	FCollisionQueryParams CollisionQueryParam = FCollisionQueryParams::DefaultQueryParam;
	CollisionQueryParam.AddIgnoredActor(GetOwner());

	for (int i = 0; i < Springs.Num(); i++)
	{
		FVector Start = Springs[i]->GetComponentLocation();
		FVector End = Start - (Springs[i]->GetUpVector() * (MaxLength + WheelRadius));

		if (GetWorld()->LineTraceSingleByChannel(Hit, Start, End, ECollisionChannel::ECC_Visibility, CollisionQueryParam))
		{
			WheelIsGrounded[i] = true;

			float CurrentSpringLength = Hit.Distance - WheelRadius;
			float SpringCompression = (RestLength - CurrentSpringLength) / SpringTravel;
			float SpringForce = SpringStiffness * SpringCompression;

			float SpringVelocity = FVector::DotProduct(CarBodyMesh->GetPhysicsLinearVelocityAtPoint(Start),
				Springs[i]->GetUpVector());
			float DampForce = DamperStiffness * SpringVelocity;

			float NetForce = SpringForce - DampForce;

			CarBodyMesh->AddForceAtLocation(NetForce * Hit.ImpactNormal, Hit.ImpactPoint);

			SetTirePosition(TireMeshes[i], Hit.ImpactPoint + Springs[i]->GetUpVector() * WheelRadius);

			DrawDebugLine(GetWorld(), Start, Hit.ImpactPoint, FColor::Red);
		}
		else
		{
			WheelIsGrounded[i] = false;

			SetTirePosition(TireMeshes[i], Springs[i]->GetComponentLocation() - Springs[i]->GetUpVector() * MaxLength);


			DrawDebugLine(GetWorld(), Start, End, FColor::Green);
		}
	}
}

void AVehicle::GroundCheck()
{
	for (int32 i = 0; i < Wheels.Num(); i++)
	{
		if (WheelIsGrounded[i])
		{
			IsGrounded = true;
			return;
		}
	}

	IsGrounded = false;
}

void AVehicle::CalculateCarVelocity()
{
	CurrentCarLocalVelocity = CarBodyMesh->GetComponentTransform().InverseTransformVector(CarBodyMesh->GetComponentVelocity());
	CarVelocityRatio = CurrentCarLocalVelocity.X / MaxSpeed;
	if (FMath::IsNearlyZero(CarVelocityRatio, 0.001f))
	{
		CarVelocityRatio = 0.0f;
	}
}

void AVehicle::MoveAcceleration()
{
	if (FMath::Abs(CurrentCarLocalVelocity.X) >= MaxSpeed)
	{
		return;
	}
	CarBodyMesh->AddForceAtLocation(Acceleration * ForwardInput * CarBodyMesh->GetForwardVector() * CarBodyMesh->GetMass(),
		AccelerationPoint->GetComponentLocation());
}

void AVehicle::MoveDecelration()
{
	CarBodyMesh->AddForceAtLocation((BreakInput ? BreakingDeceleration : Deceleration) *
		CarVelocityRatio * -CarBodyMesh->GetForwardVector() * CarBodyMesh->GetMass(),
		AccelerationPoint->GetComponentLocation());
}

void AVehicle::Turn()
{
	CarBodyMesh->AddTorqueInDegrees(SteerStrength * SteerInput *
		TurningCurve->GetFloatValue(FMath::Abs(CarVelocityRatio)) * FMath::Sign(CarVelocityRatio) * CarBodyMesh->GetUpVector(),
		FName(), true);
}

void AVehicle::Brake()
{
	float YVelocity = CurrentCarLocalVelocity.Y;

	float DragMagnitude = -YVelocity * (BreakInput ? BreakingDragCoefficient : DragCoefficient);

	FVector DragForce = CarBodyMesh->GetRightVector() * DragMagnitude * CarBodyMesh->GetMass();

	CarBodyMesh->AddForceAtLocation(DragForce, CarBodyMesh->GetCenterOfMass());
}

void AVehicle::TurnInAir()
{
	// 캐릭터의 각속도 가져오기
	FVector CurrentAngularVelocity = CarBodyMesh->GetPhysicsAngularVelocityInDegrees();

	// Z축(수직축) 회전만 고려
	float AngularVelocityZ = CurrentAngularVelocity.Y;

	// 각속도를 사용하여 회전 각도 계산
	float DeltaAngle = AngularVelocityZ * UGameplayStatics::GetWorldDeltaSeconds(this);

	TotalRotationAngle += DeltaAngle;

	int32 FullRotations = FMath::FloorToInt(FMath::Abs(TotalRotationAngle) / 360.0f);

	LastAngularVelocity = CurrentAngularVelocity;

	CarBodyMesh->AddTorqueInDegrees(InAirRotateSpeed * -SteerInput * CarBodyMesh->GetForwardVector(),
		FName(), true);

	CarBodyMesh->AddTorqueInDegrees(InAirRotateSpeed * ForwardInput * CarBodyMesh->GetRightVector(),
		FName(), true);
}

void AVehicle::Movement()
{
	if (IsGrounded)
	{
		MoveAcceleration();
		MoveDecelration();
		Turn();
		Brake();

		TotalRotationAngle = 0.0f;
		LastAngularVelocity = FVector::ZeroVector;
	}
	else
	{
		TurnInAir();
	}
}

void AVehicle::ShowTireRotate()
{
	float SteeringAngle = MaxSteeringAngle * SteerInput;

	FRotator Rot = FRotator(TireRotSpeed * -CarVelocityRatio * UGameplayStatics::GetWorldDeltaSeconds(this), 0.0, 0.0);

	for (int32 i = 0; i < TireMeshes.Num(); i++)
	{
		TireMeshes[i]->AddLocalRotation(Rot);

		// 앞바퀴
		if (i < 2)
		{
			FRotator SteerRot = Wheels[i]->GetRelativeRotation();
			SteerRot.Yaw = SteeringAngle;

			Wheels[i]->SetRelativeRotation(SteerRot);
		}
	}
}

void AVehicle::SetTirePosition(UStaticMeshComponent* Tire, FVector TargetPosition)
{
	Tire->SetWorldLocation(TargetPosition);
}

void AVehicle::ShowSkidMark()
{
	for (int32 i = 0; i < 2; i++)
	{
		if (WheelIsGrounded[i + 2] && BreakInput)
		{
			BreakSkidMarks[i]->SetActive(true);
		}
		else
		{
			BreakSkidMarks[i]->SetActive(false);
		}
	}
}

void AVehicle::Visuals()
{
	ShowTireRotate();
	ShowSkidMark();
}

void AVehicle::MoveActionInput(const FInputActionValue& Value)
{
	FVector2D Vec = Value.Get<FVector2D>();

	ForwardInput = Vec.X;
	SteerInput = Vec.Y;
}

void AVehicle::BreakActionInput(const FInputActionValue& Value)
{
	BreakInput = Value.Get<bool>();
}

