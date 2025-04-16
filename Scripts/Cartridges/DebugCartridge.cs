using Assets.Scripts;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Pipes;
using System.Collections.Generic;
using System.Text;
using System;
using Assets.Scripts.Objects;
using TMPro;
using UnityEngine;
using Assets.Scripts.Objects.Electrical;
using System.Linq;
using Entropy.Scripts.Processor;
using Entropy.Scripts.Utilities;

namespace Entropy.Scripts.Cartridges
{
    public class DebugCartridge : Cartridge
    {
        [SerializeField]
        private TextMeshProUGUI _displayTextMesh;
        public static List<DebugCartridge> AllDebugCartridges = new();
        private static string _notApplicableString = "N/A";
        private string _selectedText = string.Empty;
        private string _outputText = string.Empty;
        private Device _scannedDevice;
        private Device _lastScannedDevice;
        private bool _needTopScroll;
        private StringBuilder _stringBuilder;

        public Device ScannedDevice => !RootParent || !RootParent.HasAuthority || !CursorManager.CursorThing ? null : (CursorManager.CursorThing as Device);

        public DebugCartridge()
        {
	        this.Slots = new List<Slot>();
        }
        public override void Awake()
        {
            base.Awake();
        }
        public override void OnAssignedReference()
        {
            base.OnAssignedReference();
            AllDebugCartridges.Add(this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            AllDebugCartridges.Remove(this);
        }

        public void ReadLogicText()
        {
	        this._scannedDevice = ScannedDevice;
            lock (this._outputText)
            {
                if (this._scannedDevice != null && this._scannedDevice is ICircuitHolder circuitHolder)
                {
                    var slot = this._scannedDevice.Slots.FirstOrDefault(x => x.Type == Slot.Class.ProgrammableChip);
                    if (slot == null)
                        return;
                    if (this._lastScannedDevice != this._scannedDevice) this._needTopScroll = true;
                    this._lastScannedDevice = this._scannedDevice;
                    this._selectedText = this._scannedDevice.DisplayName.ToUpper();
                    this._stringBuilder ??= new StringBuilder();
                    this._stringBuilder.Clear();
                    if (slot.Get() is ProgrammableChip chip)
                    {
                        var processor = chip.GetExtension<ProgrammableChip, ChipProcessor>();
                        var previousLines = Math.Max(processor.Pc - 4, 0);
                        for (int i = previousLines; i < processor.Pc; i++)
                        {
	                        this._stringBuilder.AppendFormat(" {0,3}: ", i);
	                        this._stringBuilder.AppendLine(processor.GetSourceLine(i));
                        }
                        this._stringBuilder.AppendFormat(">{0,3}: ", processor.Pc);
                        this._stringBuilder.AppendLine(processor.GetSourceLine(processor.Pc));
                        for (int i = processor.Pc + 1; i < processor.Pc + 5; i++)
                        {
	                        this._stringBuilder.AppendFormat(" {0,3}: ", i);
	                        this._stringBuilder.AppendLine(processor.GetSourceLine(i));
                        }
                    }
                    this._outputText = this._stringBuilder.ToString();
                }
                else
                {
	                this._selectedText = _notApplicableString;
	                this._outputText = string.Empty;
                }
            }
        }

        public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
        {
            return base.AttackWith(attack, doAction);
        }

        public override DelayedActionInstance InteractWith(Interactable interactable, Interaction interaction, bool doAction = true)
        {
            return base.InteractWith(interactable, interaction, doAction);
        }

        public override void OnPrimaryUseStart()
        {
            base.OnPrimaryUseStart();
        }

        public override void OnPrimaryUseEnd()
        {
            base.OnPrimaryUseEnd();
        }

        public override void OnTabletScanned(Thing thing)
        {
            base.OnTabletScanned(thing);
        }

        public override void OnTabletScrollUp()
        {
            base.OnTabletScrollUp();
        }

        public override void OnTabletScrollDown()
        {
            base.OnTabletScrollDown();
            if (this._scannedDevice != null && this._scannedDevice is ICircuitHolder circuitHolder)
			{
				var slot = this._scannedDevice.Slots.FirstOrDefault(x => x.Type == Slot.Class.ProgrammableChip);
				if (slot == null)
					return;
				if (slot.Get() is ProgrammableChip chip)
				{
					var processor = chip.GetExtension<ProgrammableChip, ChipProcessor>();
                    if(processor.IsAtBreakpoint)
                        processor.DoNextStep();
                }
			}
		}

        public override void OnUsePrimary(Vector3 targetLocation, Quaternion targetRotation, ulong steamId, bool authoringMode)
        {
            base.OnUsePrimary(targetLocation, targetRotation, steamId, authoringMode);
        }

        public override DelayedActionInstance OnUseSecondary(bool doAction = false, float actionCompletedRatio = 1)
        {
            return base.OnUseSecondary(doAction, actionCompletedRatio);
        }

        public override bool TryInteractWithSlotOccupant(Interactable interactable, out DelayedActionInstance actionInstance, bool doAction = true)
        {
            return base.TryInteractWithSlotOccupant(interactable, out actionInstance, doAction);
        }

        public override void OnScreenUpdate()
        {
            base.OnScreenUpdate();
            if (this._needTopScroll)
            {
	            this._needTopScroll = false;
	            this._scrollPanel.SetScrollPosition(0.0f);
            }
            this.SelectedTitle.text = this._selectedText;
            this._displayTextMesh.text = this._outputText;
            this._scrollPanel.SetContentHeight(this._displayTextMesh.preferredHeight);
        }
    }
}
