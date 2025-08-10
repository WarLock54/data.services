using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperLibrary
{
    [Serializable]
    public class DValidationResult
    {
        private List<DValidationFailure> _errors;

        /// <summary>
        /// Whether validation succeeded
        /// </summary>
        public virtual bool IsValid => Errors.Count == 0;

        /// <summary>
        /// A collection of errors
        /// </summary>
        public List<DValidationFailure> Errors
        {
            get => _errors;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                // Ensure any nulls are removed and the list is copied
                // to be consistent with the constructor below.
                _errors = value.Where(failure => failure != null).ToList(); ;
            }
        }

        /// <summary>
        /// The RuleSets that were executed during the validation run.
        /// </summary>
        public string[] RuleSetsExecuted { get; set; }

        /// <summary>
        /// Creates a new ValidationResult
        /// </summary>
        public DValidationResult()
        {
            _errors = new List<DValidationFailure>();
        }

        /// <summary>
        /// Creates a new ValidationResult from a collection of failures
        /// </summary>
        /// <param name="failures">Collection of <see cref="ValidationFailure"/> instances which is later available through the <see cref="Errors"/> property.</param>
        /// <remarks>
        /// Any nulls will be excluded.
        /// The list is copied.
        /// </remarks>
        public DValidationResult(IEnumerable<DValidationFailure> failures)
        {
            _errors = failures.Where(failure => failure != null).ToList();
        }

        /// <summary>
        /// Creates a new ValidationResult by combining several other ValidationResults.
        /// </summary>
        /// <param name="otherResults"></param>
        public DValidationResult(IEnumerable<DValidationResult> otherResults)
        {
            _errors = otherResults.SelectMany(x => x.Errors).ToList();
            RuleSetsExecuted = otherResults.Where(x => x.RuleSetsExecuted != null).SelectMany(x => x.RuleSetsExecuted).Distinct().ToArray();
        }

        internal DValidationResult(List<DValidationFailure> errors)
        {
            _errors = errors;
        }

        /// <summary>
        /// Generates a string representation of the error messages separated by new lines.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return ToString(Environment.NewLine);
        }

        /// <summary>
        /// Generates a string representation of the error messages separated by the specified character.
        /// </summary>
        /// <param name="separator">The character to separate the error messages.</param>
        /// <returns></returns>
        public string ToString(string separator)
        {
            return string.Join(separator, _errors.Select(failure => failure.ErrorMessage));
        }

        /// <summary>
        /// Converts the ValidationResult's errors collection into a simple dictionary representation.
        /// </summary>
        /// <returns>A dictionary keyed by property name
        /// where each value is an array of error messages associated with that property.
        /// </returns>
        public IDictionary<string, string[]> ToDictionary()
        {
            return Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).ToArray()
                );
        }

        /************************************************************/
        /// <summary>
        /// Adds a new validation failure.
        /// </summary>
        /// <param name="failure">The failure to add.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void AddFailure(DValidationFailure failure)
        {
            if (failure == null) throw new ArgumentNullException(nameof(failure), "A failure must be specified when calling AddFailure");
            Errors.Add(failure);
        }

        /// <summary>
        /// Adds a new validation failure for the specified property.
        /// </summary>
        /// <param name="propertyName">The property name</param>
        /// <param name="errorMessage">The error message</param>
        public void AddFailure(string propertyName, string errorMessage)
        {
            //errorMessage.Guard("An error message must be specified when calling AddFailure.", nameof(errorMessage));
            //errorMessage = MessageFormatter.BuildMessage(errorMessage);
            AddFailure(new DValidationFailure(propertyName ?? string.Empty, errorMessage));
        }

        /// <summary>
        /// Adds a new validation failure for the specified message.
        /// The failure will be associated with the current property being validated.
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        public void AddFailure(string errorMessage)
        {
            //errorMessage.Guard("An error message must be specified when calling AddFailure.", nameof(errorMessage));
            //errorMessage = MessageFormatter.BuildMessage(errorMessage);
            AddFailure(new DValidationFailure("", errorMessage));
        }
    }

    #region License
    // Copyright (c) .NET Foundation and contributors.
    //
    // Licensed under the Apache License, Version 2.0 (the "License");
    // you may not use this file except in compliance with the License.
    // You may obtain a copy of the License at
    //
    // http://www.apache.org/licenses/LICENSE-2.0
    //
    // Unless required by applicable law or agreed to in writing, software
    // distributed under the License is distributed on an "AS IS" BASIS,
    // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    // See the License for the specific language governing permissions and
    // limitations under the License.
    //
    // The latest version of this file can be found at https://github.com/FluentValidation/FluentValidation
    #endregion


    /// <summary>
    /// Defines a validation failure
    /// </summary>
    [Serializable]
    public class DValidationFailure
    {

        /// <summary>
        /// Creates a new validation failure.
        /// </summary>
        public DValidationFailure()
        {

        }

        /// <summary>
        /// Creates a new validation failure.
        /// </summary>
        public DValidationFailure(string propertyName, string errorMessage) : this(propertyName, errorMessage, null)
        {

        }

        /// <summary>
        /// Creates a new ValidationFailure.
        /// </summary>
        public DValidationFailure(string propertyName, string errorMessage, object attemptedValue)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
            AttemptedValue = attemptedValue;
        }

        /// <summary>
        /// The name of the property.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// The property value that caused the failure.
        /// </summary>
        public object AttemptedValue { get; set; }

        /// <summary>
        /// Custom state associated with the failure.
        /// </summary>
        public object CustomState { get; set; }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string ErrorCode { get; set; }

        /// <summary>
        /// Gets or sets the formatted message placeholder values.
        /// </summary>
        public Dictionary<string, object> FormattedMessagePlaceholderValues { get; set; }

        /// <summary>
        /// Creates a textual representation of the failure.
        /// </summary>
        public override string ToString()
        {
            return ErrorMessage;
        }
    }
}
