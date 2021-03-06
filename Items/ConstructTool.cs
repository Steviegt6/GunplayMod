﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace Gunplay.Items
{
    public class ConstructTool : ModItem
    {
		public override string Texture => "Terraria/Item_1";

		//public int power/* = TCHelper.PowerFromParts(parts)*/;
		public string itemType;

		public List<string> parts;
		public Dictionary<string, int> effects;

		public ConstructTool()
		{
			//power = 0;
			itemType = "";
			parts = new List<string>();
			effects = new Dictionary<string, int>();
		}

		public override void SetDefaults()
		{
			item.width = 32;
			item.height = 32;

			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTurn = true;
			item.autoReuse = true;
			item.UseSound = SoundID.Item6;
			item.useTime = 10;
			item.useAnimation = 10;

			item.noUseGraphic = true; //hmm yes let me remove the held item layer instead

			//TerrariaConstruct.instance.Logger.Info("Power Before: " + power);
			//power = TCHelper.PowerFromParts(parts);
			//TerrariaConstruct.instance.Logger.Info("Power Mid: " + power);
			GHelper.SetDefaults(item);
			//TerrariaConstruct.instance.Logger.Info("Power After: " + power);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			foreach (TooltipLine line in tooltips)
			{
				if (line.Name == "ItemName" && line.mod == "Terraria")
				{
					line.text = GHelper.GetToolName(parts) + " " + itemType;
				}
			}

			string effectsText = "";
			foreach (KeyValuePair<string, int> pair in effects)
			{
				EffectType effectType = GHelper.GetEffectType(pair.Key);
				if (effectsText != "")
				{
					effectsText += "\n";
				}

				effectsText += $"{effectType.name}";
				if (effectType.showLevel)
				{
					effectsText += $" Level {pair.Value}";
				}
				if (Main.keyState.PressingShift())
				{
					effectsText += $": {effectType.desc}";
				}
			}
			tooltips.Add(new TooltipLine(mod, "TerrariaConstruct:EffectsLine", effectsText));
			if (!Main.keyState.PressingShift() && effects.Count > 0)
			{
				tooltips.Add(new TooltipLine(mod, "TerrariaConstruct:ShiftMessageLine", "Press Shift to see descriptions!"));
			}

			if (GHelper.CheckForBrokenParts(parts))
			{
				tooltips.Add(new TooltipLine(mod, "TerrariaConstruct:BrokenPartsLine", "This tool has broken parts! Did you disable a mod since you last played?"));
			}

			tooltips.Add(new TooltipLine(mod, "h", "Power = " + GHelper.PowerFromParts(parts)));
		}

		public override ModItem Clone(Item item)
		{
			ConstructTool myClone = (ConstructTool)base.Clone(item);
			myClone.itemType = itemType;
			myClone.parts = parts;
			myClone.effects = effects;
			return myClone;
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			item.SetNameOverride(GHelper.GetToolName(parts) + " " + itemType);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			item.SetNameOverride(GHelper.GetToolName(parts) + " " + itemType);
			return false;
		}

		public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			parts = parts.OrderBy(x => GHelper.GetPartType(x).layer).ToList();
			List<Texture2D> textures = new List<Texture2D>();
			List<Vector2> offsets = new List<Vector2>();
			foreach (string key in parts)
			{
				try
				{
					PartType partType = GHelper.GetPartType(key);
					Texture2D texture = ModContent.GetTexture(partType.useTexture);
					textures.Add(texture);
					Vector2 offset = partType.offset;
					offsets.Add(offset);
				}
				catch (Exception e)
				{
					Gunplay.Instance.Logger.Error("Something failed when getting invTexture in PostDrawInInventory! " + e.Message);
				}
			}
			for (int index = 0; index < textures.Count; index++)
			{
				Texture2D texture = textures[index];
				Vector2 offset = offsets[index];
				spriteBatch.Draw(texture, position + offset, null, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);
			}
		}

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			Vector2 position = item.position - Main.screenPosition + new Vector2(item.width / 2, item.height / 2);
			List<Texture2D> textures = new List<Texture2D>();
			List<Vector2> offsets = new List<Vector2>();
			parts = parts.OrderBy(x => GHelper.GetPartType(x).layer).ToList();
			foreach (string key in parts)
			{
				try
				{
					PartType partType = GHelper.GetPartType(key);
					Texture2D tex = ModContent.GetTexture(partType.useTexture);
					textures.Add(tex);
					Vector2 offset = partType.offset;
					offsets.Add(offset);
				}
				catch (Exception e)
				{
					Gunplay.Instance.Logger.Error("Something failed when getting invTexture in PostDrawInWorld! " + e.Message);
				}
			}
			for (int index = 0; index < textures.Count; index++)
			{
				Texture2D texture = textures[index];
				Vector2 offset = offsets[index] / 2f;
				spriteBatch.Draw(texture, position + offset, null, lightColor, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
			}
		}

		public override bool? PrefixChance(int pre, UnifiedRandom rand)
		{
			return false;
		}

		public override TagCompound Save()
		{
			return new TagCompound {
				//{"power", power },
				{"itemType", itemType },
				{"parts", parts },
				{"effectKeys", effects.Keys.ToList() },
				{"effectValues", effects.Values.ToList() }
			};
		}

		public override void Load(TagCompound tag)
		{
			//power = tag.GetInt("power");
			itemType = tag.GetString("itemType");
			parts = tag.GetList<string>("parts").ToList();
			IList<string> effectKeys = tag.GetList<string>("effectKeys");
			IList<int> effectValues = tag.GetList<int>("effectValues");
			effects = effectKeys.Zip(effectValues, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v);
			SetDefaults();
		}

		public override void NetSend(BinaryWriter writer)
		{
			//writer.Write(power);
			writer.Write(itemType);
			writer.Write(parts.Count);
			for (int i = 0; i < parts.Count; i++)
			{
				writer.Write(parts[i]);
			}
			writer.Write(effects.Count);
			for (int i = 0; i < effects.Count; i++)
			{
				writer.Write(effects.ElementAt(i).Key);
				writer.Write(effects.ElementAt(i).Value);
			}
		}

		public override void NetRecieve(BinaryReader reader)
		{
			parts = new List<string>();
			effects = new Dictionary<string, int>();

			//power = reader.ReadInt32();
			itemType = reader.ReadString();
			int partCount = reader.ReadInt32();
			for (int i = 0; i < partCount; i++)
			{
				parts.Add(reader.ReadString());
			}
			int effectCount = reader.ReadInt32();
			for (int i = 0; i < effectCount; i++)
			{
				string key = reader.ReadString();
				int value = reader.ReadInt32();
				effects.Add(key, value);
			}
			SetDefaults();
		}
	}
}
