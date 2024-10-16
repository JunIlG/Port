

#pragma once

#include "CoreMinimal.h"
#include "GameFramework/Pawn.h"
#include "InputActionValue.h"
#include "Vehicle.generated.h"

UCLASS()
class ARCADECAR_API AVehicle : public APawn
{
	GENERATED_BODY()

public:
	AVehicle();

protected:
	virtual void BeginPlay() override;

public:	
	virtual void Tick(float DeltaTime) override;

	virtual void SetupPlayerInputComponent(class UInputComponent* PlayerInputComponent) override;

protected:
	void Suspension();
	void GroundCheck();
	void CalculateCarVelocity();
	void MoveAcceleration();
	void MoveDecelration();
	void Turn();
	void Brake();
	void TurnInAir();
	void Movement();

	void ShowTireRotate();
	void SetTirePosition(UStaticMeshComponent* Tire, FVector TargetPosition);
	void ShowSkidMark();
	void Visuals();

	void MoveActionInput(const FInputActionValue& Value);
	void BreakActionInput(const FInputActionValue& Value);

protected:
	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	TObjectPtr<UStaticMeshComponent> CarBodyMesh;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	TObjectPtr<USceneComponent> AccelerationPoint;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	TArray<TObjectPtr<USceneComponent>> Wheels;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	TArray<TObjectPtr<USceneComponent>> Springs;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	TArray<TObjectPtr<UStaticMeshComponent>> TireMeshes;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	TObjectPtr<class USpringArmComponent> SpringArm;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	TObjectPtr<class UCameraComponent> Camera;

	UPROPERTY(EditDefaultsOnly, BlueprintReadOnly)
	TArray<TObjectPtr<class UNiagaraComponent>> BreakSkidMarks;

	TArray<bool> WheelIsGrounded;

	bool IsGrounded = false;

	UPROPERTY(EditAnywhere, Category = "Suspension Setting")
	float SpringStiffness = 2500000.0f;

	UPROPERTY(EditAnywhere, Category = "Suspension Setting")
	float DamperStiffness = 10000.0f;

	UPROPERTY(EditAnywhere, Category = "Suspension Setting")
	float RestLength = 60.0f;

	UPROPERTY(EditAnywhere, Category = "Suspension Setting")
	float SpringTravel = 10.0f;

	UPROPERTY(EditAnywhere, Category = "Suspension Setting")
	float WheelRadius = 40.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Car Setting")
	float Acceleration = 2500.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Car Setting")
	float Deceleration = 1500.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Car Setting")
	float SteerStrength = 3000.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Car Setting")
	UCurveFloat* TurningCurve;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Car Setting")
	float MaxSpeed = 10000.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Car Setting")
	float DragCoefficient = 1.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Car Setting")
	float BreakingDeceleration = 10000.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Car Setting")
	float BreakingDragCoefficient = 0.5f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Car Setting")
	float InAirRotateSpeed = 600.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Visualize")
	float TireRotSpeed = 1000.0f;

	UPROPERTY(EditAnywhere, BlueprintReadWrite, Category = "Visualize")
	float MaxSteeringAngle = 35.0f;

	UPROPERTY(EditDefaultsOnly, Category = "Input")
	class UInputMappingContext* VehicleContext;

	UPROPERTY(EditDefaultsOnly, Category = "Input")
	class UInputAction* MoveAction;

	UPROPERTY(EditDefaultsOnly, Category = "Input")
	class UInputAction* BreakAction;

private:
	float ForwardInput;
	float SteerInput;
	bool BreakInput;

	FVector CurrentCarLocalVelocity = FVector::ZeroVector;

	float CarVelocityRatio = 0.0f;

	float TotalRotationAngle = 0.0f;

	FVector LastAngularVelocity;
};
