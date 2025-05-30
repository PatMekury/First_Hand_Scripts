/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using UnityEngine;

namespace Oculus.Interaction
{
    /// <summary>
    /// PointerInteractor provides a base template for any kind of interaction which can be characterized as, "pointing at something."
    /// Interactors of this kind, examples of which include <see cref="PokeInteractor"/> and <see cref="RayInteractor"/>, emit
    /// <see cref="PointerEvent"/>s which describe their behavior during interaction, in addition to the signals and data provided by
    /// <see cref="Interactor{TInteractor, TInteractable}"/>s.
    /// </summary>
    /// <remarks>
    /// Like <see cref="Interactor{TInteractor, TInteractable}"/>, this type has a
    /// [curiously recurring](https://en.wikipedia.org/wiki/Curiously_recurring_template_pattern) generic argument
    /// <typeparamref name="TInteractor"/>, which should be the concrete interactor type which derives from this type and is uniquely
    /// associated with <typeparamref name="TInteractable"/>.
    /// </remarks>
    public abstract class PointerInteractor<TInteractor, TInteractable> : Interactor<TInteractor, TInteractable>
                                    where TInteractor : Interactor<TInteractor, TInteractable>
                                    where TInteractable : PointerInteractable<TInteractor, TInteractable>
    {
        protected void GeneratePointerEvent(PointerEventType pointerEventType, TInteractable interactable)
        {
            Pose pose = ComputePointerPose();

            if (interactable == null)
            {
                return;
            }

            if (interactable.PointableElement != null)
            {
                if (pointerEventType == PointerEventType.Hover)
                {
                    interactable.PointableElement.WhenPointerEventRaised +=
                        HandlePointerEventRaised;
                }
                else if (pointerEventType == PointerEventType.Unhover)
                {
                    interactable.PointableElement.WhenPointerEventRaised -=
                        HandlePointerEventRaised;
                }
            }

            interactable.PublishPointerEvent(
                new PointerEvent(Identifier, pointerEventType, pose, Data));
        }

        protected virtual void HandlePointerEventRaised(PointerEvent evt)
        {
            if (evt.Identifier == Identifier &&
                evt.Type == PointerEventType.Cancel &&
                Interactable != null)
            {
                TInteractable interactable = Interactable;
                interactable.RemoveInteractorByIdentifier(Identifier);
                interactable.PointableElement.WhenPointerEventRaised -=
                    HandlePointerEventRaised;
            }
        }

        protected override void InteractableSet(TInteractable interactable)
        {
            base.InteractableSet(interactable);
            GeneratePointerEvent(PointerEventType.Hover, interactable);
        }

        protected override void InteractableUnset(TInteractable interactable)
        {
            GeneratePointerEvent(PointerEventType.Unhover, interactable);
            base.InteractableUnset(interactable);
        }

        protected override void InteractableSelected(TInteractable interactable)
        {
            base.InteractableSelected(interactable);
            GeneratePointerEvent(PointerEventType.Select, interactable);
        }

        protected override void InteractableUnselected(TInteractable interactable)
        {
            GeneratePointerEvent(PointerEventType.Unselect, interactable);
            base.InteractableUnselected(interactable);
        }

        protected override void DoPostprocess()
        {
            base.DoPostprocess();
            if (_interactable != null)
            {
                GeneratePointerEvent(PointerEventType.Move, _interactable);
            }
        }

        protected abstract Pose ComputePointerPose();
    }
}
