var MoveInput = (function ($) {
	'use strict';

	$ = $ && $.hasOwnProperty('default') ? $['default'] : $;

	/*
	 *  Copyright 2014 Gary Green.
	 *  Licensed under the Apache License, Version 2.0 (the "License");
	 *  you may not use this file except in compliance with the License.
	 *  You may obtain a copy of the License at
	 *
	 *  http://www.apache.org/licenses/LICENSE-2.0
	 *
	 *  Unless required by applicable law or agreed to in writing, software
	 *  distributed under the License is distributed on an "AS IS" BASIS,
	 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
	 *  See the License for the specific language governing permissions and
	 *  limitations under the License.
	 */

	function MoveInput(table, options) {

		this.init(table, options);

	}
	MoveInput.prototype = {

		defaults: {
			namespace: 'moveinput', // Namespace for the keybinding, etc
			beforeMove: $.noop,      // Function to call before navigating
			afterMove: $.noop,       // Function to call after navigating
			listenTarget: 'input',   // Listen for move/key events from this target
			focusTarget: 'input',    // Focus this target after the move has completed
			enabledKeys: ['tab', 'enter'], // Key's enabled
			continuousDelay: 50      // Delay in milliseconds for continuous movement when holding down arrow keys
		},

		KEYS: {
			9: 'tab',
			13: 'enter'
		},

		/**
		 * Initialise the plugin
		 * @param  {element} table   Table element
		 * @param  {object} options  Custom plugin options
		 * @return {void}
		 */
		init: function (table, options) {

			this.options = $.extend({}, this.defaults, options);
			this.$table = $(table);

			// Bind main plugin events
			this.bindEvents();
		},

		findMoveTarget: function ($element) {

			var $row = $element.closest('tbody').find('td:has(' + this.options.focusTarget + ')');
			var $current = $element.closest('td');
			var $currentIndex = $row.index($current);

			// Get the move to td container
			return $row.eq($currentIndex + 1 < $row.length ? $currentIndex + 1 : $currentIndex);
		},

		/**
		 * Handle moving from one td to another and focussing the target
		 * @param  {DOMElement} Current dom element 
		 * @param  {string} Direction to move
		 * @return {void}
		 */
		move: function (element, direction) {

			var $this = $(element);

			var findMoveTarget = $.proxy(function () {
				return this.findMoveTarget($this);
			}, this);

			// Allow move to not happen if beforeMove function returns 'false'
			var move = this.options.beforeMove($this[0], findMoveTarget, direction);
			if (move === false) {
				return;
			}

			var $target = findMoveTarget();

			if ($target.length) {
				// Focus the target
				this.focusTarget($target);

				// Let the afterMove callback know we're finished
				this.options.afterMove($this[0], $target[0], direction);
			}
		},

		/**
		 * Focus the input target
		 * @param  {jQuery} $target
		 * @return {void}
		 */
		focusTarget: function ($target) {
			$target.find(this.options.focusTarget).focus();
		},

		/**
		 * Bind main plugin events
		 * @return {void}
		 */
		bindEvents: function () {
			var moveTimer;

			var moveEvent = function (event) {

				var direction = this.KEYS[event.which];

				// Check the key/direction is enabled
				if ($.inArray(direction, this.options.enabledKeys) === -1) {
					return;
				}

				if (this.options.continuousDelay > 0) {
					if (moveTimer) {
						return;
					}

					moveTimer = setTimeout(function () {
						moveTimer = null;
					}, this.options.continuousDelay);
				}

				event.preventDefault();
				this.move(event.target, direction);
			};

			var keyup = function () {
				moveTimer = null;
			};

			this.$table
				.on('keydown.' + this.options.namespace, this.options.listenTarget, $.proxy(moveEvent, this))
				.on('keyup.' + this.options.namespace, this.options.listenTarget, keyup);
		},

		/**
		 * Unbind main plugin events
		 * @return {void}
		 */
		unbindEvents: function () {
			this.$table.off('.' + this.options.namespace);
		},

		/**
		 * Destroy the plugin
		 * @return {self}
		 */
		destroy: function () {
			this.unbindEvents();
			return this;
		}

	};

	$.fn.moveInput = function (options) {

		options = options || {};

		var namespace = options.namespace || MoveInput.prototype.defaults.namespace,
			isMethodCall = typeof options === 'string';

		return $(this).each(function () {

			var $this = $(this);

			// Get plugin instance
			var moveInput = $this.data(namespace);

			if (isMethodCall) {
				if (!moveInput) return this;

				switch (options) {
					case 'destroy':
						moveInput.destroy();
						$this.removeData(namespace);
						break;

				}

				return this;
			}

			// Initialise?
			if (moveInput === undefined) {
				moveInput = new MoveInput(this, options);
				$this.data(namespace, moveInput);
			}
			else {
				// Reinitialise moveInput
				moveInput.destroy().init(this, options);
			}

			return this;

		});

	};

	return MoveInput;

}(jQuery));
