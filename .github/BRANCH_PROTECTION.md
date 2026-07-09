# Branch Protection Setup Guide

This guide explains how to configure GitHub branch protection rules to ensure PRs can't be merged until all checks pass.

## Quick Setup

1. **Go to your repository on GitHub**
   - Navigate to: https://github.com/erenken/gateMonitor

2. **Open Settings**
   - Click on **Settings** tab
   - In the left sidebar, click **Branches** (under "Code and automation")

3. **Add Branch Protection Rule**
   - Click **Add rule** or **Add branch protection rule**

4. **Configure the Rule**

   **Branch name pattern:**
   ```
   main
   ```

   **Check these options:**
   - ☑️ **Require a pull request before merging**
     - ☑️ Require approvals: `1` (if you want review)
     - ☑️ Dismiss stale pull request approvals when new commits are pushed

   - ☑️ **Require status checks to pass before merging**
     - ☑️ Require branches to be up to date before merging
     - **Add required status checks:**
       1. Type: `.NET Build & Test`
       2. Type: `Angular Build & Test`
       3. Type: `All Checks Passed`

       > **Note**: These checks will only appear after you've run the workflow at least once. Push your current changes first, then come back to add these checks.

   - ☑️ **Require conversation resolution before merging** (optional but recommended)

   - ☑️ **Do not allow bypassing the above settings** (if you want strict enforcement)

5. **Save the Rule**
   - Scroll to the bottom and click **Create** or **Save changes**

## What This Does

With branch protection enabled:
- ❌ **Can't merge** if `.NET Build & Test` fails
- ❌ **Can't merge** if `Angular Build & Test` fails
- ❌ **Can't merge** if `All Checks Passed` fails
- ✅ **Can merge** only when all checks are green

## Workflow Checks Explained

### .NET Build & Test
- Builds the .NET solution (`dotnet/GateMonitor.slnx`)
- Runs all unit tests
- Publishes test results

### Angular Build & Test
- Installs npm dependencies
- Builds the Angular app and library
- Runs Karma/Jasmine tests in headless Chrome
- Runs ESLint (non-blocking)

### All Checks Passed
- Final status check that verifies both jobs succeeded
- This is the single check you can use for branch protection if you prefer

## Testing Your Setup

1. **Push changes** to trigger the workflow:
   ```bash
   git push origin work/vibeItUp
   ```

2. **Create a PR** to `main`:
   - Go to GitHub and create a pull request
   - You'll see the checks running

3. **Add required checks** (after first run):
   - Go back to Settings → Branches → Edit rule
   - The status checks will now be available in the dropdown

## Troubleshooting

### Check not appearing in the dropdown?
- Make sure the workflow has run at least once
- The check name must match exactly (case-sensitive)
- Wait a few minutes for GitHub to index the checks

### Checks failing?
- Click "Details" next to the failing check
- Review the logs to see what failed
- Common issues:
  - Missing dependencies
  - Test failures
  - Build errors
  - Linting issues

### Need to bypass temporarily?
- If you're an admin, you can temporarily disable the rule
- Or add "Allow administrators to bypass" in the settings
- **Not recommended** for production branches

## GitHub CLI Alternative

If you prefer command-line setup:

```bash
# Install GitHub CLI: https://cli.github.com/

# Create branch protection rule
gh api repos/erenken/gateMonitor/branches/main/protection \
  --method PUT \
  --field required_status_checks[strict]=true \
  --field required_status_checks[contexts][]=".NET Build & Test" \
  --field required_status_checks[contexts][]="Angular Build & Test" \
  --field required_status_checks[contexts][]="All Checks Passed" \
  --field enforce_admins=true \
  --field required_pull_request_reviews[required_approving_review_count]=0
```

## Additional Security (Optional)

Consider also enabling:
- **Require signed commits** - Ensures commit authenticity
- **Require deployments to succeed** - If you have deployment checks
- **Restrict who can push** - Limit direct pushes to certain teams/users
- **Require linear history** - Enforces rebase or squash merges

## Learn More

- [GitHub Branch Protection Documentation](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)
- [Status Checks](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/collaborating-on-repositories-with-code-quality-features/about-status-checks)
