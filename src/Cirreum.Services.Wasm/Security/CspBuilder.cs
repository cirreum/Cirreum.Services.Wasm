namespace Cirreum.Security;

using System.Text;

/// <summary>
/// Implementation of the <see cref="ICspBuilder"/> that provides the ability
/// to configure and build a content-security-policy.
/// </summary>
/// <remarks>
/// <para>
/// The default configuration is as follows:
/// </para>
/// base-uri 'self';
/// upgrade-insecure-requests;
/// default-src none;
/// object-src 'none;
/// img-src data: https:;
/// connect-src 'self';
/// script-src 'self';
/// style-src 'self';
/// manifest-src 'self';
/// form-action 'self';
/// </remarks>
sealed class CspBuilder : ICspBuilder {

	private readonly Dictionary<string, List<string>> _cspRules = new Dictionary<string, List<string>> {
		{ "base-uri", new List<string>{ "'self'"} },
		{ "upgrade-insecure-requests", new List<string>{ "" } },
		{ "default-src", new List<string>{ "'none'"} },
		{ "object-src", new List<string>{ "'none'" } },
		{ "child-src", new List<string>{ "'none'" } },
		{ "worker-src", new List<string>{ "'none'" } },
		{ "media-src", new List<string>{ "data:", "https:" } },
		{ "img-src", new List<string>{ "data:", "https:" } },
		{ CspDirectives.CONNECT_SRC, new List<string>{ "'self'" } },
		{ CspDirectives.SCRIPT_SRC, new List<string>{ "'self'" } },
		{ CspDirectives.STYLE_SRC, new List<string>{ "'self'" } },
		{ CspDirectives.MANIFEST_SRC, new List<string>{ "'self'" } },
		{ CspDirectives.FORM_ACTION, new List<string>{ "'self'" } }
	};

	/// <summary>
	/// Based on the directive and sources configured, generates a
	/// content-security-policy string.
	/// </summary>
	/// <returns>A <see cref="string"/> containing the Csp value.</returns>
	public override string ToString() {
		return this.Build();
	}

	/// <inheritdoc/>
	public string Build() {

		var policyBuilder = new StringBuilder();
		var srcBuilder = new StringBuilder();
		foreach (var src in _cspRules) {
			srcBuilder.Append($"{src.Key} ");
			srcBuilder.AppendJoin(" ", src.Value);
			srcBuilder.Append(';');
			srcBuilder.AppendLine();
			policyBuilder.Append(srcBuilder);
			srcBuilder.Clear();
		}

		return policyBuilder.ToString();

	}

	/// <inheritdoc/>
	public List<string> GetSources(string directive) {

		if (_cspRules.TryGetValue(directive, out var sources)) {
			return sources;
		}

		return [];

	}

	/// <inheritdoc/>
	public ICspBuilder AddSource(string directive, string source) {
		if (_cspRules.TryGetValue(directive, out var sources)) {
			if (sources.Contains(source)) {
				return this;
			}
			sources.Add(source);
			return this;
		}
		_cspRules.Add(directive, [source]);
		return this;
	}

	/// <inheritdoc/>
	public bool ContainsDirective(string directive) {
		return _cspRules.ContainsKey(directive);
	}

	/// <inheritdoc/>
	public bool ContainsSource(string directive, string source) {
		return _cspRules.TryGetValue(directive, out var sources) && sources.Contains(source);
	}

	/// <inheritdoc/>
	public void RemoveSource(string directive, string source) {
		if (_cspRules.TryGetValue(directive, out var sources)) {
			sources.Remove(source);
		}
	}

	/// <inheritdoc/>
	public void ClearDirective(string directive) {
		_cspRules.Remove(directive);
	}

	/// <inheritdoc/>
	public void Reset() {
		_cspRules.Clear();
	}

}