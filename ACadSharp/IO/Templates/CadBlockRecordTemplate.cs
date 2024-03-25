﻿using ACadSharp.Blocks;
using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Tables;
using CSUtilities.Extensions;
using System.Collections.Generic;

namespace ACadSharp.IO.Templates
{
	internal class CadBlockRecordTemplate : CadTableEntryTemplate<BlockRecord>
	{
		public ulong? FirstEntityHandle { get; set; }

		public ulong? LastEntityHandle { get; set; }

		public ulong? BeginBlockHandle { get; set; }

		public ulong? EndBlockHandle { get; set; }

		public ulong? LayoutHandle { get; set; }

		public List<ulong> OwnedObjectsHandlers { get; set; } = new List<ulong>();

		public List<ulong> InsertHandles { get; set; } = new List<ulong>();

		public string LayerName { get; set; }

		public CadBlockRecordTemplate() : base(new BlockRecord()) { }

		public CadBlockRecordTemplate(BlockRecord block) : base(block) { }

		public override void Build(CadDocumentBuilder builder)
		{
			base.Build(builder);

			if (builder.TryGetCadObject(this.LayoutHandle, out Layout layout))
			{
				this.CadObject.Layout = layout;
			}

			if (this.FirstEntityHandle.HasValue)
			{
				var entities = this.getEntitiesCollection<Entity>(builder, this.FirstEntityHandle.Value, this.LastEntityHandle.Value);
				this.CadObject.Entities.AddRange(entities);
			}
			else
			{
				foreach (ulong handle in this.OwnedObjectsHandlers)
				{
					if (builder.TryGetCadObject<Entity>(handle, out Entity child))
					{
						switch (child)
						{
							case Viewport viewport:
								this.CadObject.Viewports.Add(viewport);
								break;
							default:
								this.CadObject.Entities.Add(child);
								break;
						}

					}
				}
			}
		}

		public void SetBlockToRecord(CadDocumentBuilder builder)
		{
			if (builder.TryGetCadObject(this.BeginBlockHandle, out Block block))
			{
				if (!block.Name.IsNullOrEmpty())
				{
					this.CadObject.Name = block.Name;
				}

				block.Flags = this.CadObject.BlockEntity.Flags;
				block.BasePoint = this.CadObject.BlockEntity.BasePoint;
				block.XrefPath = this.CadObject.BlockEntity.XrefPath;
				block.Comments = this.CadObject.BlockEntity.Comments;

				this.CadObject.BlockEntity = block;

			}

			if (builder.TryGetCadObject(this.EndBlockHandle, out BlockEnd blockEnd))
			{
				this.CadObject.BlockEnd = blockEnd;
			}
		}
	}
}
