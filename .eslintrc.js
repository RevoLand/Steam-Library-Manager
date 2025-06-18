module.exports = {
  root: true,
  env: {
    browser: true,
    es2021: true,
    node: true,
  },
  parser: '@typescript-eslint/parser',
  parserOptions: {
    ecmaVersion: 'latest',
    sourceType: 'module',
  },
  plugins: ['@typescript-eslint', 'react', 'react-hooks', 'eslint-plugin-prettier'],
  extends: [
    'eslint:recommended',
    'plugin:react/recommended',
    'plugin:@typescript-eslint/eslint-recommended',
    'plugin:@typescript-eslint/recommended',
    'plugin:import/recommended',
    'plugin:import/electron',
    'plugin:import/typescript',
  ],
  settings: {
    'import/resolver': {
      typescript: {
        alwaysTryTypes: true,
        project: ['./tsconfig.json'],
      },
    },
    react: {
      version: 'detect',
    },
  },
  ignorePatterns: ['dist', 'out'],
  rules: {
    'prettier/prettier': 'warn',

    /* React & jsx related rules */
    'react/prop-types': 'off',
    'react/react-in-jsx-scope': 'off',
    'react/no-access-state-in-setstate': 'warn',
    'react/no-did-mount-set-state': 'warn',
    'react/no-did-update-set-state': 'warn',
    'react/no-this-in-sfc': 'warn',
    'react/jsx-curly-spacing': [
      'warn',
      {
        when: 'never',
        allowMultiline: true,
      },
    ],
    'react/jsx-equals-spacing': ['warn', 'never'],
    'react/jsx-first-prop-new-line': ['warn', 'multiline'],
    'react/jsx-no-useless-fragment': ['warn', { allowExpressions: true }],
    'react/jsx-tag-spacing': ['warn'],
    'react/jsx-boolean-value': ['warn'],
    'react/jsx-closing-bracket-location': ['warn', 'line-aligned'], // https://github.com/jsx-eslint/eslint-plugin-react/blob/master/docs/rules/jsx-closing-bracket-location.md
    'jsx-quotes': ['warn', 'prefer-single'], // https://eslint.org/docs/rules/jsx-quotes
    'react-hooks/exhaustive-deps': 'off',
    'react/display-name': 'warn',
    'react/no-unescaped-entities': 'warn',

    /* Rules */
    'no-empty': ['error', { allowEmptyCatch: true }],
    'no-useless-catch': 'warn',
    'no-useless-escape': 'warn',
    'no-async-promise-executor': ['warn'],
    curly: 'warn', // https://eslint.org/docs/rules/curly
    'no-new-wrappers': 'warn', // https://eslint.org/docs/rules/no-new-wrappers
    'block-scoped-var': 'warn', // https://eslint.org/docs/rules/block-scoped-var
    'no-shadow': 'off', // https://eslint.org/docs/rules/no-shadow
    '@typescript-eslint/no-shadow': 'warn',
    'no-else-return': 'warn', // https://eslint.org/docs/rules/no-else-return
    'no-label-var': ['warn'], // https://eslint.org/docs/rules/no-label-var
    'no-multi-spaces': ['warn'], // https://eslint.org/docs/rules/no-multi-spaces
    'no-return-assign': ['warn'], // https://eslint.org/docs/rules/no-return-assign
    'no-trailing-spaces': ['warn'], // https://eslint.org/docs/rules/no-trailing-spaces
    'no-undef-init': ['warn'], // https://eslint.org/docs/rules/no-undef-init
    'no-useless-computed-key': ['warn'], // https://eslint.org/docs/rules/no-useless-computed-key
    'no-useless-rename': ['warn'], // https://eslint.org/docs/rules/no-useless-rename
    'no-whitespace-before-property': ['warn'], // https://eslint.org/docs/rules/no-whitespace-before-property
    'rest-spread-spacing': ['warn'], // https://eslint.org/docs/rules/rest-spread-spacing
    'no-return-await': 'warn', // https://eslint.org/docs/rules/no-return-await
    'no-self-compare': 'warn', // https://eslint.org/docs/rules/no-self-compare
    'no-useless-concat': 'warn', // https://eslint.org/docs/rules/no-useless-concat
    'no-use-before-define': 'warn', // https://eslint.org/docs/rules/no-use-before-define

    /* Styling */
    'padding-line-between-statements': [
      'warn',
      {
        // return öncesinde her zaman boşluk olmalı
        blankLine: 'always',
        prev: '*',
        next: 'return',
      },
      {
        // değişken tanımlamalarından sonra her zaman boşluk olmalı
        blankLine: 'always',
        prev: ['const', 'let', 'var'],
        next: '*',
      },
      {
        // değişken tanımlamalarından önce de boşluk içermeden değişken tanımlanabilmeli
        blankLine: 'any',
        prev: ['const', 'let', 'var'],
        next: ['const', 'let', 'var'],
      },
    ],
    'array-bracket-spacing': ['warn', 'never'], // https://eslint.org/docs/rules/array-bracket-spacing
    'object-property-newline': ['warn', { allowAllPropertiesOnSameLine: true }], // https://eslint.org/docs/rules/object-property-newline
    'block-spacing': 'warn', // https://eslint.org/docs/rules/block-spacing
    'brace-style': ['warn', '1tbs', { allowSingleLine: true }], // https://eslint.org/docs/rules/brace-style
    camelcase: ['warn'], // https://eslint.org/docs/rules/camelcase
    'new-cap': ['warn'], // https://eslint.org/docs/rules/new-cap
    'new-parens': ['warn'], // https://eslint.org/docs/rules/new-parens
    'wrap-iife': ['warn'], // https://eslint.org/docs/rules/wrap-iife
    yoda: ['warn'], // https://eslint.org/docs/rules/yoda
    'consistent-return': ['warn'], // https://eslint.org/docs/rules/consistent-return
    'comma-spacing': [
      'warn',
      {
        before: false,
        after: true,
      },
    ], // https://eslint.org/docs/rules/comma-spacing
    'comma-style': ['warn', 'last'], // https://eslint.org/docs/rules/comma-style
    'func-call-spacing': 'warn', // https://eslint.org/docs/rules/func-call-spacing
    'function-call-argument-newline': ['warn', 'consistent'], // https://eslint.org/docs/rules/function-call-argument-newline
    'key-spacing': [
      'warn',
      {
        beforeColon: false,
        afterColon: true,
        mode: 'strict',
      },
    ], // https://eslint.org/docs/rules/key-spacing
    'keyword-spacing': [
      'warn',
      {
        before: true,
        after: true,
      },
    ], // https://eslint.org/docs/rules/keyword-spacing
    'lines-between-class-members': ['warn'], // https://eslint.org/docs/rules/lines-between-class-members
    'newline-per-chained-call': ['warn', { ignoreChainWithDepth: 3 }], // https://eslint.org/docs/rules/newline-per-chained-call
    'no-lonely-if': ['warn'], // https://eslint.org/docs/rules/no-lonely-if
    'no-multiple-empty-lines': ['warn', { max: 2 }], // https://eslint.org/docs/rules/no-multiple-empty-lines
    'no-unneeded-ternary': ['warn'], // https://eslint.org/docs/rules/no-unneeded-ternary
    'no-param-reassign': ['warn'], // https://eslint.org/docs/rules/no-param-reassign
    'object-curly-spacing': ['warn', 'always'], // https://eslint.org/docs/rules/object-curly-spacing
    'operator-linebreak': ['warn'], // https://eslint.org/docs/rules/operator-linebreak
    semi: ['warn'], // https://eslint.org/docs/rules/semi
    'semi-spacing': ['warn'], // https://eslint.org/docs/rules/semi-spacing
    'semi-style': ['warn'], // https://eslint.org/docs/rules/semi-style
    'space-unary-ops': ['warn'], // https://eslint.org/docs/rules/space-unary-ops
    'space-before-blocks': ['warn'], // https://eslint.org/docs/rules/space-before-blocks
    'space-in-parens': ['warn', 'never'], // https://eslint.org/docs/rules/space-in-parens
    'space-infix-ops': ['warn'], // https://eslint.org/docs/rules/space-infix-ops
    'switch-colon-spacing': ['warn'], // https://eslint.org/docs/rules/switch-colon-spacing
    'spaced-comment': ['warn', 'always'], // https://eslint.org/docs/rules/spaced-comment
    'arrow-spacing': ['warn'], // https://eslint.org/docs/rules/arrow-spacing
    'no-duplicate-imports': ['warn'], // https://eslint.org/docs/rules/no-duplicate-imports
    'template-curly-spacing': ['warn'], // https://eslint.org/docs/rules/template-curly-spacing
    'no-nested-ternary': ['warn'], // https://eslint.org/docs/rules/no-nested-ternary
    quotes: ['warn', 'single'], // https://eslint.org/docs/rules/quotes
    'no-case-declarations': 'off',
    'no-unused-vars': 'off',
    '@typescript-eslint/no-unused-vars': 'warn',
  },
};
