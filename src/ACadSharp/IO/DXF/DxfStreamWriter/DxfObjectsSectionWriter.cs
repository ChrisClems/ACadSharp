﻿using ACadSharp.Entities;
using ACadSharp.Objects;
using ACadSharp.Objects.Evaluations;
using CSUtilities.Converters;
using System;
using System.Linq;

namespace ACadSharp.IO.DXF
{
	internal class DxfObjectsSectionWriter : DxfSectionWriterBase
	{
		public override string SectionName { get { return DxfFileToken.ObjectsSection; } }

		public DxfObjectsSectionWriter(IDxfStreamWriter writer, CadDocument document, CadObjectHolder holder, DxfWriterConfiguration configuration)
			: base(writer, document, holder, configuration)
		{
		}

		protected override void writeSection()
		{
			while (this.Holder.Objects.Any())
			{
				CadObject item = this.Holder.Objects.Dequeue();

				this.writeObject(item);
			}
		}

		protected void writeObject<T>(T co)
			where T : CadObject
		{
			switch (co)
			{
				case AcdbPlaceHolder:
				case EvaluationGraph:
				case Material:
				case MultiLeaderAnnotContext:
				case VisualStyle:
				case ImageDefinitionReactor:
				case UnknownNonGraphicalObject:
				case XRecord:
					this.notify($"Object not implemented : {co.GetType().FullName}");
					return;
			}


			if (co is XRecord && !this.Configuration.WriteXRecords)
			{
				return;
			}

			this._writer.Write(DxfCode.Start, co.ObjectName);

			this.writeCommonObjectData(co);

			switch (co)
			{
				case BookColor bookColor:
					this.writeBookColor(bookColor);
					return;
				case CadDictionary cadDictionary:
					this.writeDictionary(cadDictionary);
					return;
				case DictionaryVariable dictvar:
					this.writeDictionaryVariable(dictvar);
					break;
				case Group group:
					this.writeGroup(group);
					break;
				case ImageDefinition imageDefinition:
					this.writeImageDefinition(imageDefinition);
					return;
				case Layout layout:
					this.writeLayout(layout);
					break;
				case MLineStyle mlStyle:
					this.writeMLineStyle(mlStyle);
					break;
				case MultiLeaderStyle multiLeaderlStyle:
					this.writeMultiLeaderStyle(multiLeaderlStyle);
					break;
				case PlotSettings plotSettings:
					this.writePlotSettings(plotSettings);
					break;
				case Scale scale:
					this.writeScale(scale);
					break;
				case SortEntitiesTable sortensTable:
					this.writeSortentsTable(sortensTable);
					break;
				case XRecord record:
					this.writeXRecord(record);
					break;
				default:
					throw new NotImplementedException($"Object not implemented : {co.GetType().FullName}");
			}

			this.writeExtendedData(co.ExtendedData);
		}

		protected void writeBookColor(BookColor color)
		{
			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.DbColor);

			this._writer.Write(62, color.Color.GetApproxIndex());
			this._writer.WriteTrueColor(420, color.Color);
			this._writer.Write(430, color.Name);
		}

		protected void writeDictionary(CadDictionary e)
		{
			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.Dictionary);

			this._writer.Write(280, e.HardOwnerFlag);
			this._writer.Write(281, (int)e.ClonningFlags);

			foreach (NonGraphicalObject item in e)
			{
				if (item is XRecord && !this.Configuration.WriteXRecords)
				{
					return;
				}

				this._writer.Write(3, item.Name);
				this._writer.Write(350, item.Handle);
			}

			//Add the entries as objects
			foreach (CadObject item in e)
			{
				this.Holder.Objects.Enqueue(item);
			}
		}

		protected void writeDictionaryVariable(DictionaryVariable dictvar)
		{
			DxfClassMap map = DxfClassMap.Create<DictionaryVariable>();

			this._writer.Write(100, DxfSubclassMarker.DictionaryVariables);

			this._writer.Write(1, dictvar.Value, map);
			this._writer.Write(280, dictvar.ObjectSchemaNumber, map);
		}

		protected void writePlotSettings(PlotSettings plot)
		{
			DxfClassMap map = DxfClassMap.Create<PlotSettings>();

			this._writer.Write(100, DxfSubclassMarker.PlotSettings);

			this._writer.Write(1, plot.PageName, map);
			this._writer.Write(2, plot.SystemPrinterName, map);

			this._writer.Write(4, plot.PaperSize, map);

			this._writer.Write(6, plot.PlotViewName, map);
			this._writer.Write(7, plot.StyleSheet, map);

			this._writer.Write(40, plot.UnprintableMargin.Left, map);
			this._writer.Write(41, plot.UnprintableMargin.Bottom, map);
			this._writer.Write(42, plot.UnprintableMargin.Right, map);
			this._writer.Write(43, plot.UnprintableMargin.Top, map);
			this._writer.Write(44, plot.PaperWidth, map);
			this._writer.Write(45, plot.PaperHeight, map);
			this._writer.Write(46, plot.PlotOriginX, map);
			this._writer.Write(47, plot.PlotOriginY, map);
			this._writer.Write(48, plot.WindowLowerLeftX, map);
			this._writer.Write(49, plot.WindowLowerLeftY, map);

			this._writer.Write(140, plot.WindowUpperLeftX, map);
			this._writer.Write(141, plot.WindowUpperLeftY, map);
			this._writer.Write(142, plot.NumeratorScale, map);
			this._writer.Write(143, plot.DenominatorScale, map);

			this._writer.Write(70, (short)plot.Flags, map);

			this._writer.Write(72, (short)plot.PaperUnits, map);
			this._writer.Write(73, (short)plot.PaperRotation, map);
			this._writer.Write(74, (short)plot.PlotType, map);
			this._writer.Write(75, plot.ScaledFit, map);
			this._writer.Write(76, (short)plot.ShadePlotMode, map);
			this._writer.Write(77, (short)plot.ShadePlotResolutionMode, map);
			this._writer.Write(78, plot.ShadePlotDPI, map);
			this._writer.Write(147, plot.PrintScale, map);

			this._writer.Write(148, plot.PaperImageOrigin.X, map);
			this._writer.Write(149, plot.PaperImageOrigin.Y, map);
		}

		protected void writeScale(Scale scale)
		{
			this._writer.Write(100, DxfSubclassMarker.Scale);

			this._writer.Write(70, 0);
			this._writer.Write(300, scale.Name);
			this._writer.Write(140, scale.PaperUnits);
			this._writer.Write(141, scale.DrawingUnits);
			this._writer.Write(290, scale.IsUnitScale ? (short)1 : (short)0);
		}

		protected void writeGroup(Group group)
		{
			this._writer.Write(100, DxfSubclassMarker.Group);

			this._writer.Write(300, group.Description);
			this._writer.Write(70, group.IsUnnamed ? (short)1 : (short)0);
			this._writer.Write(71, group.Selectable ? (short)1 : (short)0);

			foreach (Entity entity in group.Entities.Values)
			{
				this._writer.WriteHandle(340, entity);
			}
		}

		protected void writeImageDefinition(ImageDefinition definition)
		{
			DxfClassMap map = DxfClassMap.Create<ImageDefinition>();

			this._writer.Write(100, DxfSubclassMarker.RasterImageDef);

			this._writer.Write(90, definition.ClassVersion, map);
			this._writer.Write(1, definition.FileName, map);

			this._writer.Write(10, definition.Size, map);

			this._writer.Write(280, definition.IsLoaded ? 1 : 0, map);

			this._writer.Write(281, (byte)definition.Units, map);
		}

		protected void writeLayout(Layout layout)
		{
			DxfClassMap map = DxfClassMap.Create<Layout>();

			this.writePlotSettings(layout);

			this._writer.Write(100, DxfSubclassMarker.Layout);

			this._writer.Write(1, layout.Name, map);

			//this._writer.Write(70, (short) 1,map);
			this._writer.Write(71, layout.TabOrder, map);

			this._writer.Write(10, layout.MinLimits, map);
			this._writer.Write(11, layout.MaxLimits, map);
			this._writer.Write(12, layout.InsertionBasePoint, map);
			this._writer.Write(13, layout.Origin, map);
			this._writer.Write(14, layout.MinExtents, map);
			this._writer.Write(15, layout.MaxExtents, map);
			this._writer.Write(16, layout.XAxis, map);
			this._writer.Write(17, layout.YAxis, map);

			this._writer.Write(146, layout.Elevation, map);

			this._writer.Write(76, (short)0, map);

			this._writer.WriteHandle(330, layout.AssociatedBlock, map);
		}

		protected void writeMLineStyle(MLineStyle style)
		{
			DxfClassMap map = DxfClassMap.Create<MLineStyle>();

			this._writer.Write(100, DxfSubclassMarker.MLineStyle);

			this._writer.Write(2, style.Name, map);

			this._writer.Write(70, (short)style.Flags, map);

			this._writer.Write(3, style.Description, map);

			this._writer.Write(62, style.FillColor.GetApproxIndex(), map);

			this._writer.Write(51, style.StartAngle, map);
			this._writer.Write(52, style.EndAngle, map);
			this._writer.Write(71, (short)style.Elements.Count, map);
			foreach (MLineStyle.Element element in style.Elements)
			{
				this._writer.Write(49, element.Offset, map);
				this._writer.Write(62, element.Color.Index, map);
				this._writer.Write(6, element.LineType.Name, map);
			}
		}

		protected void writeMultiLeaderStyle(MultiLeaderStyle style)
		{
			DxfClassMap map = DxfClassMap.Create<MultiLeaderStyle>();

			this._writer.Write(100, DxfSubclassMarker.MLeaderStyle);

			this._writer.Write(179, 2);
			//	this._writer.Write(2, style.Name, map);
			this._writer.Write(170, (short)style.ContentType, map);
			this._writer.Write(171, (short)style.MultiLeaderDrawOrder, map);
			this._writer.Write(172, (short)style.LeaderDrawOrder, map);
			this._writer.Write(90, style.MaxLeaderSegmentsPoints, map);
			this._writer.Write(40, style.FirstSegmentAngleConstraint, map);
			this._writer.Write(41, style.SecondSegmentAngleConstraint, map);
			this._writer.Write(173, (short)style.PathType, map);
			this._writer.WriteCmColor(91, style.LineColor, map);
			this._writer.WriteHandle(340, style.LeaderLineType);
			this._writer.Write(92, (short)style.LeaderLineWeight, map);
			this._writer.Write(290, style.EnableLanding, map);
			this._writer.Write(42, style.LandingGap, map);
			this._writer.Write(291, style.EnableDogleg, map);
			this._writer.Write(43, style.LandingDistance, map);
			this._writer.Write(3, style.Description, map);
			this._writer.WriteHandle(341, style.Arrowhead);
			this._writer.Write(44, style.ArrowheadSize, map);
			this._writer.Write(300, style.DefaultTextContents, map);
			this._writer.WriteHandle(342, style.TextStyle);
			this._writer.Write(174, (short)style.TextLeftAttachment, map);
			this._writer.Write(178, (short)style.TextRightAttachment, map);
			this._writer.Write(175, style.TextAngle, map);
			this._writer.Write(176, (short)style.TextAlignment, map);
			this._writer.WriteCmColor(93, style.TextColor, map);
			this._writer.Write(45, style.TextHeight, map);
			this._writer.Write(292, style.TextFrame, map);
			this._writer.Write(297, style.TextAlignAlwaysLeft, map);
			this._writer.Write(46, style.AlignSpace, map);
			this._writer.WriteHandle(343, style.BlockContent);
			this._writer.WriteCmColor(94, style.BlockContentColor, map);

			//	Write 3 doubles since group codes do not conform vector group codes
			this._writer.Write(47, style.BlockContentScale.X, map);
			this._writer.Write(49, style.BlockContentScale.Y, map);
			this._writer.Write(140, style.BlockContentScale.Z, map);

			this._writer.Write(293, style.EnableBlockContentScale, map);
			this._writer.Write(141, style.BlockContentRotation, map);
			this._writer.Write(294, style.EnableBlockContentRotation, map);
			this._writer.Write(177, (short)style.BlockContentConnection, map);
			this._writer.Write(142, style.ScaleFactor, map);
			this._writer.Write(295, style.OverwritePropertyValue, map);
			this._writer.Write(296, style.IsAnnotative, map);
			this._writer.Write(143, style.BreakGapSize, map);
			this._writer.Write(271, (short)style.TextAttachmentDirection, map);
			this._writer.Write(272, (short)style.TextBottomAttachment, map);
			this._writer.Write(273, (short)style.TextTopAttachment, map);
			this._writer.Write(298, false); //	undocumented
		}

		private void writeSortentsTable(SortEntitiesTable e)
		{
			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.SortentsTable);

			this._writer.WriteHandle(330, e.BlockOwner);

			foreach (SortEntitiesTable.Sorter item in e.Sorters)
			{
				this._writer.WriteHandle(331, item.Entity);
				this._writer.Write(5, item.Handle);
			}
		}

		protected void writeXRecord(XRecord e)
		{
			this._writer.Write(DxfCode.Subclass, DxfSubclassMarker.XRecord);

			foreach (var item in e.Entries)
			{
				this._writer.Write(item.Code, item.Value);
			}
		}
	}
}
