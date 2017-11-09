using System;
using CScape.Core.Extensions;
using CScape.Core.Game.Entity;
using CScape.Core.Game.Item;
using CScape.Models.Data;
using CScape.Models.Game.Item;
using JetBrains.Annotations;

namespace CScape.Core.Network.Entity.Utility
{
    public class PlayerAppearanceSerialier
    {
        public const int MaxAppearanceUpdateSize = 64;

        public Blob SerializedAppearance { get; } = new Blob(MaxAppearanceUpdateSize);

        public PlayerAppearanceSerialier()
        {
            
        }

        public void SerializeNewAppearance(
            [NotNull] string username, 
            PlayerAppearance appearance,
            [NotNull] PlayerEquipmentContainer equipment)
        {
            if (username == null) throw new ArgumentNullException(nameof(username));
            if (equipment == null) throw new ArgumentNullException(nameof(equipment));

            var sizePlaceholder = BeginWriteApperance();

            WriteGender(appearance);
            WriteOverheads();
            WriteBodyObjects(appearance, equipment);
            WriteBodyColors(appearance);
            WriteAnimationIndices();
            WriteUsername(username);
            WriteCombatLevel();
            WriteSkillLevel();

            EndWriteAppearance(sizePlaceholder);
        }

        private void WriteBodyObjects(PlayerAppearance appearance, IItemContainer equipment)
        {
            /* 
            * todo : some equipped items conflict with body parts 
            * write body model if chest doesn't conceal the body
            * write head model if head item doesn't fully conceal the head.
            * write beard model if head item doesn't fully conceal the head.
            * abstract out all of that logic
            */

            const int equipSlotSize = 12;
            for (var i = 0; i < equipSlotSize; i++)
            {
                const short plrObjMagic = 0x100;
                const short itemMagic = 0x200;

                if (!equipment.Provider[i].IsEmpty())
                    SerializedAppearance.Write16((short)(equipment.Provider[i].Id.ItemId + itemMagic));
                else
                {
                    switch (i)
                    {
                        case 4:
                            SerializedAppearance.Write16((short)(appearance.Chest + plrObjMagic));
                            break;
                        case 6:
                            SerializedAppearance.Write16((short)(appearance.Arms + plrObjMagic));
                            break;
                        case 7:
                            SerializedAppearance.Write16((short)(appearance.Legs + plrObjMagic));
                            break;
                        case 8:
                            SerializedAppearance.Write16((short)(appearance.Head + plrObjMagic));
                            break;
                        case 9:
                            SerializedAppearance.Write16((short)(appearance.Hands + plrObjMagic));
                            break;
                        case 10:
                            SerializedAppearance.Write16((short)(appearance.Feet + plrObjMagic));
                            break;
                        case 11:
                            SerializedAppearance.Write16((short)(appearance.Beard + plrObjMagic));
                            break;
                        default:
                            SerializedAppearance.Write(0);
                            break;
                    }
                }
            }
        }

        private BlobPlaceholder BeginWriteApperance()
        {
            SerializedAppearance.ResetWrite();
            return SerializedAppearance.Placeholder(1);
        }

        private void WriteGender(PlayerAppearance appearance)
        {
            SerializedAppearance.Write((byte)appearance.Gender);
        }

        private void WriteOverheads()
        {
            // TODO : overheads
            SerializedAppearance.Write(0);
        }

        private void WriteBodyColors(PlayerAppearance appearance)
        {
            SerializedAppearance.Write(appearance.HairColor);
            SerializedAppearance.Write(appearance.TorsoColor);
            SerializedAppearance.Write(appearance.LegColor);
            SerializedAppearance.Write(appearance.FeetColor);
            SerializedAppearance.Write(appearance.SkinColor);
        }

        private void WriteAnimationIndices()
        {
            SerializedAppearance.Write16(0x328); // standAnimIndex
            SerializedAppearance.Write16(0x337); // standTurnAnimIndex
            SerializedAppearance.Write16(0x333); // walkAnimIndex
            SerializedAppearance.Write16(0x334); // turn180AnimIndex
            SerializedAppearance.Write16(0x335); // turn90CWAnimIndex
            SerializedAppearance.Write16(0x336); // turn90CCWAnimIndex
            SerializedAppearance.Write16(0x338); // runAnimIndex
        }

        private void WriteUsername(string username)
        {
            SerializedAppearance.Write64(StringToLong(username));
        }

        private static long StringToLong(string s)
        {
            var l = 0L;

            foreach (var c in s)
            {
                l *= 37L;
                if (c >= 'A' && c <= 'Z') l += 1 + c - 65;
                else if (c >= 'a' && c <= 'z') l += 1 + c - 97;
                else if (c >= '0' && c <= '9') l += 27 + c - 48;
            }

            while (l % 37L == 0L && l != 0L)
                l /= 37L;

            return l;
        }

        private void WriteCombatLevel()
        {
            SerializedAppearance.Write(3); // todo : cmb
        }

        private void WriteSkillLevel()
        {
            SerializedAppearance.Write16(0); // todo : skill
        }


        private static void EndWriteAppearance(BlobPlaceholder sizePlaceholder)
        {
            sizePlaceholder.WriteSize();
        }
    }
}
