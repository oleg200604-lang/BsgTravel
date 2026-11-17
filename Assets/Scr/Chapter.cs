
using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class Chapter
{
    public int mood;
    public List<Mood> moods = new List<Mood>();

    public PhysicalParameters physicalParameters = new PhysicalParameters();
    public List<BodyParts> body = new List<BodyParts>();
    public List<Need> needs;
    public Inventory inventory;

    public void Spawn()
    {
        InitializeBody();
        BodyUpdate();
    }

    public void InitializeBody()
    {
        foreach (BodyPart part in System.Enum.GetValues(typeof(BodyPart)))
        {
            bool exists = body.Exists(x => x.bodyPart == part);

            if (!exists)
            {
                body.Add(new BodyParts { name = part.ToString(), bodyPart = part, health = 100, maxHealth = 100, power = 100, modifier = false });
            }
        }
    }

    public void Tick()
    {
        mood = 0;

        for (int i = 0; i < moods.Count; i++)
        {
            mood += moods[i].power;

            if (moods[i].timer > 0)
            {
                moods[i].timer--;
            }
            else
            {
                moods.RemoveAt(i);
                i--;
            }
        }
        BodyUpdate();
    }

    public void BodyUpdate()
    {
        if (physicalParameters == null) physicalParameters = new PhysicalParameters();

        // Свідомість
        physicalParameters.consciousness = GetEfficiency(BodyPart.brain);

        // Рухливість
        physicalParameters.moving = Average(BodyPart.legLeft, BodyPart.legRight);

        // Маніпуляція
        physicalParameters.manipulation = Average(BodyPart.armLeft, BodyPart.armRight);

        // Мовлення
        physicalParameters.talking = Average(BodyPart.jaw, BodyPart.teeth, BodyPart.tongue);

        // Слух
        physicalParameters.hearing = Average(BodyPart.earLeft, BodyPart.earRight);

        // Зір
        physicalParameters.sight = Average(BodyPart.eyeLeft, BodyPart.eyeRight);

        // Дихання
        physicalParameters.breathing = Average(BodyPart.lungLeft, BodyPart.lungRight);

        // Кровообіг
        physicalParameters.bloodPumping = GetEfficiency(BodyPart.heart);

        // Фільтрація крові
        physicalParameters.bloodFiltration = Average(BodyPart.kidneyLeft, BodyPart.kidneyRight);

        // Травлення
        physicalParameters.digestion = Average(BodyPart.stomach, BodyPart.intestine, BodyPart.liver);
    }

    private int GetEfficiency(BodyPart part)
    {
        BodyParts organ = body.Find(x => x.bodyPart == part);

        if (organ == null || organ.maxHealth <= 0)
            return 0;

        return Mathf.Clamp(
            Mathf.RoundToInt((float)organ.health / organ.maxHealth * 100),
            0, 100
        );
    }

    private int Average(params BodyPart[] parts)
    {
        if (parts.Length == 0)
            return 0;

        int total = 0;

        foreach (BodyPart part in parts)
            total += GetEfficiency(part);

        return total / parts.Length;
    }
}


[System.Serializable]
public class Mood
{
    public string name;
    public string description;
    public int power;
    public int timer;
}

[System.Serializable]
public class BodyParts
{
    public string name;
    public int health;
    public int maxHealth; 
    public BodyPart bodyPart;
    public int power;
    public bool modifier;
}

[System.Serializable]
public class PhysicalParameters
{
    public int consciousness = 0;
    public int moving = 0;
    public int manipulation = 0;
    public int talking = 0;
    public int hearing = 0;
    public int sight = 0;
    public int breathing = 0;
    public int bloodPumping = 0;
    public int bloodFiltration = 0;
    public int digestion = 0;

    // Фізичні характеристики
    public int pain = 0;
    public int blood = 100;
}

[System.Serializable]
public class Skills
{
    public int shooting;       // Стрільба
    public int melee;          // Ближній бій
    public int engineering;    // Інженерія
    public int farming;        // Рослинництво
    public int research;       // Дослідництво
    public int cooking;        // Кулінарія
    public int medicine;       // Медицина
    public int charisma;       // Харизма
    public int skillMax;        
}
[System.Serializable]
public class SkillsXP
{
    public int shooting;       
    public int melee;          
    public int engineering;    
    public int farming;        
    public int research;       
    public int cooking;        
    public int medicine;       
    public int charisma;
    public int[] skillXP;
}

[System.Serializable]
public class Need
{
    public int power;
    public int powerMax;
    public int powerExpenditure;
    public NeedType needType;
}

public class Disease
{
    public string name;
    public string description;
    public int power;
    public int effect;
    public bool chronic;
    public BodyPart bodyPart;
}

public enum NeedType
{
    hunger, thirst, sleep, alcohol, social, energy, love, comfort, drugs, smoking
}
public enum BodyPart
{
    // Голова
    head, brain, jaw, teeth, tongue, nose,

    // Очі та вуха
    eyeLeft, eyeRight,
    earLeft, earRight,

    // Тулуб
    torso,

    // Руки
    armLeft, armRight,

    // Ноги
    legLeft, legRight,

    // Органи
    heart, lungLeft, lungRight, liver, kidneyLeft, kidneyRight, stomach, intestine
}

public enum SkillsType
{
    shooting, melee, engineering, farming, research, cooking, medicine, charisma
}

