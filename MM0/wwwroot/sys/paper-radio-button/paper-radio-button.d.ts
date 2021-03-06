/**
 * DO NOT EDIT
 *
 * This file was automatically generated by
 *   https://github.com/Polymer/gen-typescript-declarations
 *
 * To modify these typings, edit the source file(s):
 *   paper-radio-button.html
 */

/// <reference path="../polymer/types/polymer.d.ts" />
/// <reference path="../paper-behaviors/paper-checked-element-behavior.d.ts" />
/// <reference path="../paper-styles/default-theme.d.ts" />
/// <reference path="../iron-flex-layout/iron-flex-layout.d.ts" />

/**
 * Material design: [Radio button](https://www.google.com/design/spec/components/selection-controls.html#selection-controls-radio-button)
 *
 * `paper-radio-button` is a button that can be either checked or unchecked.
 * User can tap the radio button to check or uncheck it.
 *
 * Use a `<paper-radio-group>` to group a set of radio buttons.  When radio buttons
 * are inside a radio group, exactly one radio button in the group can be checked
 * at any time.
 *
 * Example:
 *
 *     <paper-radio-button></paper-radio-button>
 *     <paper-radio-button>Item label</paper-radio-button>
 *
 * ### Styling
 *
 * The following custom properties and mixins are available for styling:
 *
 * Custom property | Description | Default
 * ----------------|-------------|----------
 * `--paper-radio-button-unchecked-background-color` | Radio button background color when the input is not checked | `transparent`
 * `--paper-radio-button-unchecked-color` | Radio button color when the input is not checked | `--primary-text-color`
 * `--paper-radio-button-unchecked-ink-color` | Selected/focus ripple color when the input is not checked | `--primary-text-color`
 * `--paper-radio-button-checked-color` | Radio button color when the input is checked | `--primary-color`
 * `--paper-radio-button-checked-ink-color` | Selected/focus ripple color when the input is checked | `--primary-color`
 * `--paper-radio-button-size` | Size of the radio button | `16px`
 * `--paper-radio-button-ink-size` | Size of the ripple | `48px`
 * `--paper-radio-button-label-color` | Label color | `--primary-text-color`
 * `--paper-radio-button-label-spacing` | Spacing between the label and the button | `10px`
 * `--paper-radio-button-radio-container` | A mixin applied to the internal radio container | `{}`
 * `--paper-radio-button-label` | A mixin applied to the internal label | `{}`
 * `--paper-radio-button-label-checked` | A mixin applied to the internal label when the radio button is checked | `{}`
 *
 * This element applies the mixin `--paper-font-common-base` but does not import `paper-styles/typography.html`.
 * In order to apply the `Roboto` font to this element, make sure you've imported `paper-styles/typography.html`.
 */
interface PaperRadioButtonElement extends Polymer.Element, Polymer.PaperCheckedElementBehavior {
  ariaActiveAttribute: string|null|undefined;
  hostAttributes: object|null;
  attached(): void;
  ready(): void;
}

interface HTMLElementTagNameMap {
  "paper-radio-button": PaperRadioButtonElement;
}
