using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(NewProjectile))]
public class ProjectileEditor : UnityEditor.Editor
{
    SerializedProperty explosionPrefab;

    /////////////////////////////////////////////
    SerializedProperty dieOnExplosion;
    SerializedProperty continuousHits;
    SerializedProperty destroyDestructables;
    SerializedProperty rotationOverLifetime;
    SerializedProperty followTargetDuration;
    SerializedProperty canBeAbsorbed;

    SerializedProperty slowDown;
    SerializedProperty minRandomSlowdown;
    SerializedProperty maxRandomSlowdown;

    /////////////////////////////////////////////
    SerializedProperty terrainBehavior;
    SerializedProperty explosionPointTerrain;
    SerializedProperty explosionOffsetTerrain;
    SerializedProperty timeBetweenExplosionTerrain;

    SerializedProperty lifetime;
    SerializedProperty explodeOnLifeEnd;

    SerializedProperty speedMultOnRoll;
    SerializedProperty rollDuration;
    SerializedProperty sizeOverRoll;
    SerializedProperty explodeOnRollEnd;

    SerializedProperty minimumAngle;
    SerializedProperty speedMultOnDeflect;
    SerializedProperty sizeMultOnDeflect;
    SerializedProperty maxBounces;
    SerializedProperty explodeOnFailBounce;

    SerializedProperty speedMultOnPenetrate;

    /////////////////////////////////////////////
    SerializedProperty characterBehvior;

    SerializedProperty ignoreOwner;

    SerializedProperty explosionPointCharacter;
    SerializedProperty explosionOffsetCharacter;
    SerializedProperty timeBetweenHitsCharacter;

    /////////////////////////////////////////////
    SerializedProperty projectileBehavior;

    SerializedProperty ignoreFromOwner;

    SerializedProperty explosionPointProjectile;
    SerializedProperty explosionOffsetProjectile;
    SerializedProperty timeBetweenExplosionProjectile;

    SerializedProperty addToDamage;
    SerializedProperty addToSize;


    private void OnEnable()
    {
        explosionPrefab = serializedObject.FindProperty("explosionPrefab");

        dieOnExplosion = serializedObject.FindProperty("dieOnExplosion");
        continuousHits = serializedObject.FindProperty("continuousHits");
        destroyDestructables = serializedObject.FindProperty("destroyDestructables");
        rotationOverLifetime = serializedObject.FindProperty("rotationOverLifetime");
        followTargetDuration = serializedObject.FindProperty("followTargetDuration");
        canBeAbsorbed = serializedObject.FindProperty("canBeAbsorbed");

        slowDown = serializedObject.FindProperty("slowDown");
        minRandomSlowdown = serializedObject.FindProperty("minRandomSlowdown");
        maxRandomSlowdown = serializedObject.FindProperty("maxRandomSlowdown");

        terrainBehavior = serializedObject.FindProperty("terrainBehavior");
        explosionPointTerrain = serializedObject.FindProperty("explosionPointTerrain");
        explosionOffsetTerrain = serializedObject.FindProperty("explosionOffsetTerrain");
        timeBetweenExplosionTerrain = serializedObject.FindProperty("timeBetweenExplosionTerrain");

        lifetime = serializedObject.FindProperty("timeBetweenExplosionTerrain");
        explodeOnLifeEnd = serializedObject.FindProperty("timeBetweenExplosionTerrain");

        speedMultOnRoll = serializedObject.FindProperty("speedMultOnRoll");
        rollDuration = serializedObject.FindProperty("rollDuration");
        sizeOverRoll = serializedObject.FindProperty("sizeOverRoll");
        explodeOnRollEnd = serializedObject.FindProperty("explodeOnRollEnd");

        minimumAngle = serializedObject.FindProperty("minimumAngle");
        speedMultOnDeflect = serializedObject.FindProperty("speedMultOnDeflect");
        sizeMultOnDeflect = serializedObject.FindProperty("sizeMultOnDeflect");
        maxBounces = serializedObject.FindProperty("maxBounces");
        explodeOnFailBounce = serializedObject.FindProperty("explodeOnFailBounce");

        speedMultOnPenetrate = serializedObject.FindProperty("speedMultOnPenetrate");

        characterBehvior = serializedObject.FindProperty("characterBehvior");

        ignoreOwner = serializedObject.FindProperty("ignoreOwner");

        explosionPointCharacter = serializedObject.FindProperty("explosionPointCharacter");
        explosionOffsetCharacter = serializedObject.FindProperty("explosionOffsetCharacter");
        timeBetweenHitsCharacter = serializedObject.FindProperty("timeBetweenHitsCharacter");

        projectileBehavior = serializedObject.FindProperty("projectileBehavior");

        ignoreFromOwner = serializedObject.FindProperty("ignoreFromOwner");

        explosionPointProjectile = serializedObject.FindProperty("explosionPointProjectile");
        explosionOffsetProjectile = serializedObject.FindProperty("explosionOffsetProjectile");
        timeBetweenExplosionProjectile = serializedObject.FindProperty("timeBetweenExplosionProjectile");

        addToDamage = serializedObject.FindProperty("addToDamage");
        addToSize = serializedObject.FindProperty("addToSize");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        NewProjectile projectile = (NewProjectile)target;

        GUILayout.Space(20);
        GUILayout.Box("Prefabs");
        EditorGUILayout.ObjectField(explosionPrefab);

        GUILayout.Space(20);
        GUILayout.Box("Settings");
        EditorGUILayout.PropertyField(dieOnExplosion);

        if (projectile.dieOnExplosion == false)
            EditorGUILayout.PropertyField(continuousHits);
        EditorGUILayout.PropertyField(destroyDestructables);
        EditorGUILayout.PropertyField(rotationOverLifetime);
        EditorGUILayout.PropertyField(followTargetDuration);
        EditorGUILayout.PropertyField(canBeAbsorbed);

        EditorGUILayout.PropertyField(slowDown);
        if (projectile.slowDown)
        {
            EditorGUILayout.PropertyField(minRandomSlowdown);
            EditorGUILayout.PropertyField(maxRandomSlowdown);
        }

        GUILayout.Space(20);
        GUILayout.Box("Terrain Collision");
        EditorGUILayout.PropertyField(terrainBehavior);

        switch (projectile.terrainBehavior)
        {
            case NewProjectile.TerrainBehavior.Explode:
                EditorGUILayout.PropertyField(explosionPointTerrain);
                EditorGUILayout.PropertyField(explosionOffsetTerrain);
                if (projectile.continuousHits)
                    EditorGUILayout.PropertyField(timeBetweenExplosionTerrain);
                break;

            case NewProjectile.TerrainBehavior.Stop:
                EditorGUILayout.PropertyField(lifetime);
                EditorGUILayout.PropertyField(explodeOnLifeEnd);
                break;

            case NewProjectile.TerrainBehavior.Roll:
                EditorGUILayout.PropertyField(speedMultOnRoll);
                EditorGUILayout.PropertyField(rollDuration);
                EditorGUILayout.PropertyField(sizeOverRoll);
                EditorGUILayout.PropertyField(explodeOnRollEnd);
                break;

            case NewProjectile.TerrainBehavior.Deflect:
                EditorGUILayout.PropertyField(minimumAngle);
                EditorGUILayout.PropertyField(speedMultOnDeflect);
                EditorGUILayout.PropertyField(sizeMultOnDeflect);
                EditorGUILayout.PropertyField(maxBounces);
                EditorGUILayout.PropertyField(explodeOnFailBounce);
                break;

            case NewProjectile.TerrainBehavior.Penetrate:
                EditorGUILayout.PropertyField(speedMultOnPenetrate);
                break;
        }


        GUILayout.Space(20);
        GUILayout.Box("Character Collision");
        EditorGUILayout.PropertyField(characterBehvior);

        EditorGUILayout.PropertyField(ignoreOwner);

        switch (projectile.characterBehvior)
        {
            case NewProjectile.CharacterBehavior.Explode:
                EditorGUILayout.PropertyField(explosionPointCharacter);
                EditorGUILayout.PropertyField(explosionOffsetCharacter);
                if (projectile.continuousHits)
                    EditorGUILayout.PropertyField(timeBetweenHitsCharacter);
                break;

            case NewProjectile.CharacterBehavior.Consume:
                break;

            case NewProjectile.CharacterBehavior.Kill:
                break;
        }

        GUILayout.Space(20);
        GUILayout.Box("Projectile Collision");
        EditorGUILayout.PropertyField(projectileBehavior);

        projectile.ignoreFromOwner = EditorGUILayout.Toggle("Ignore Projectiles From Owner", projectile.ignoreFromOwner);

        switch (projectile.projectileBehavior)
        {
            case NewProjectile.ProjectileBehavior.Explode:
                EditorGUILayout.PropertyField(explosionPointProjectile);
                EditorGUILayout.PropertyField(explosionOffsetProjectile);
                if (projectile.continuousHits)
                    EditorGUILayout.PropertyField(timeBetweenExplosionProjectile);
                break;

            case NewProjectile.ProjectileBehavior.Absorb:
                EditorGUILayout.PropertyField(addToDamage);
                EditorGUILayout.PropertyField(addToSize);
                break;

            case NewProjectile.ProjectileBehavior.Ignore:
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
